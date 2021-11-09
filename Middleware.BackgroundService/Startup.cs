using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Middleware.BackgroundService.Services;
using Middleware.Core.DAO;
using Middleware.Service;
using Middleware.Service.DAO;
using Middleware.Service.Fakes;
using Middleware.Service.FIServices;
using Middleware.Service.Processors;
using Middleware.Service.WalletServices;

namespace Middleware.BackgroundService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
       
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddScoped<ICaseDAO, CaseDAO>();
            services.AddScoped<IDocumentDAO, DocumentDAO>();
            services.AddScoped<IReversalDAO, ReversalDAO>();
            services.AddScoped<Core.DAO.ITransactionDAO, Core.DAO.TransactionDAO>();
            services.AddScoped<IReversalLogDAO, ReversalLogDAO>();
            services.AddScoped<IInstitutionDAO, InstitutionDAO>();
            services.AddScoped<IUnitOfWorkSession, UnitOfWorkSession>();

            //Service Layer
            services.AddScoped< Middleware.Service.Processors.IReversalService,Middleware.Service.Implementations. ReversalService>();
            services.AddScoped<ICaseTransmitter, CaseTransmitter>();


            //Concrete
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ILocalAccountService, LocalClient>();
            services.AddScoped<IExternalTransferService, ExternalClient>();

            //Fakes
            //services.AddSingleton<IExternalTransferService, FakeInterBankService>();
            //services.AddSingleton<ILocalAccountService, FakeBankService>();
            //services.AddSingleton<IWalletService, FakeWalletService>();

            var connection = Configuration.GetConnectionString("ApplicationConnection");
            services.AddScoped<IDbConnection>((s) => new SqlConnection(connection));

            services.Configure<ProcessMakerSettings>(opt => Configuration.GetSection("ProcessMakerSettings").Bind(opt));
            services.Configure<ReversalSettings>(opt => Configuration.GetSection("ReversalSettings").Bind(opt));
            services.Configure<FISettings>(opt => Configuration.GetSection("FISettings").Bind(opt));
            services.Configure<WalletConfigSettings>(opt => Configuration.GetSection("WalletConfigSettings").Bind(opt));
            services.Configure<SystemSettings>(opt => Configuration.GetSection("SystemSettings").Bind(opt));
            services.Configure<StatementServiceSettings>(opt => Configuration.GetSection("StatementServiceSettings").Bind(opt));

            services.AddHttpClient("HttpMessageHandler").ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    UseDefaultCredentials = true,
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, policy) =>
                    {
                        return true;
                    }
                };
            });
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                }));

            services.AddHangfireServer();
 
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHangfireDashboard("/fbnmobile/jobs");
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        public void ConfigureJobs(IServiceProvider serviceProvider)
        {
            var caseTransmitter = serviceProvider.GetRequiredService<ICaseTransmitter>();
            var reversalService = serviceProvider.GetRequiredService< Middleware.BackgroundService.Services. IReversalService>();


            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }

            RecurringJob.AddOrUpdate("Transmit cases", () => caseTransmitter.TransmitCases(), "*/5 * * * *");
            RecurringJob.AddOrUpdate("Update documents", () => caseTransmitter.TransmitUpdatedDocuments(), "*/10 * * * *");
            RecurringJob.AddOrUpdate("Wallet-based reversal", () => reversalService.ReverseWalletTransactions(), "*/5 * * * *");
            RecurringJob.AddOrUpdate("Account-based reversal", () => reversalService.ReverseAccountTransactions(), "*/5 * * * *");

        }
    }
}
