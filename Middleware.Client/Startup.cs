using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Middleware.Client.Filters;
using Newtonsoft.Json.Converters;

using Microsoft.OpenApi.Models;

using Middleware.Core.DAO;

using Middleware.Service;
using Middleware.Service.BAP;
using Middleware.Service.Beneficiary;
using Middleware.Service.DAO;
using Middleware.Service.Fakes;
using Middleware.Service.FIServices;
using Middleware.Service.Implementations;
using Middleware.Service.InterWalletServices;
using Middleware.Service.Onboarding;
using Middleware.Service.Processors;
using Middleware.Service.SmsService;
using Middleware.Service.Utilities;
using Middleware.Service.WalletServices;

using System.Data;


using System.Net.Http;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Middleware.Client.Extensions;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using Middleware.Core.Model;
using System.Text.Json.Serialization;
using Middleware.Core.DTO;
using Middleware.Service.TwofactorAuthentications;
using Middleware.Client.BaseServces;

namespace Middleware.Client
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
            services.AddHttpContextAccessor();

            //services.AddControllers(opt=> { opt.Filters.AddService<UserActivityFilter>(); })
            //  services.AddAutoMapper(typeof(MapperProfiles));
            services.AddControllers()
                 .AddJsonOptions(opts =>
                 {
                     opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                 })
              .AddNewtonsoftJson(options =>
              {
                  options.SerializerSettings.Converters.Add(new StringEnumConverter());
                  options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
              });

            //var serviceProvider = services.BuildServiceProvider();
            //var logger = serviceProvider.GetService<ILogger<Startup>>();
            //services.AddSingleton(typeof(ILogger), logger);
            //---Mock implementations
            //services.AddSingleton<IAuthenticator, FakeAuthenticator>();

            //services.AddSingleton<ICustomerDAO, FakeDAO>();
            //services.AddSingleton<ISessionDAO, FakeDAO>();
            //services.AddSingleton<IInstitutionDAO, FakeDAO>();
            //services.AddSingleton<IDeviceDAO, FakeDAO>();
            //services.AddSingleton<IUserActivityDAO, FakeDAO>();
            //services.AddSingleton<ILimitDAO, FakeDAO>();
            //services.AddSingleton<ITransactionDAO, FakeDAO>();
            //services.AddSingleton<ITransactionTrackerDAO, FakeDAO>();
            //services.AddSingleton<IWalletOpeningRequestDAO, FakeDAO>();


            //services.AddSingleton<ILocalAccountService, FakeBankService>();
            //services.AddSingleton<IBeneficiaryService, FakeBeneficiaryService>();
            //services.AddSingleton<ILimitService, FakeLimitService>();
            //services.AddScoped<IUnitOfWorkSession, FakeUnitOfWorkSession>();


            //services.AddSingleton<ISubsidiaryService, FakeSubsidiaryService>();
            //services.AddSingleton<IExternalTransferService, FakeInterBankService>();

            ///  services.AddSingleton<ILanguageConfigurationProvider, FakeLanguageProvider>();

            //services.AddSingleton<IProfileManager, FakeProfileManager>();


            services.AddSingleton<INotifier, FakeNotifier>();
            services.AddSingleton<IPaymentManager, FakePaymentManager>();

            //services.AddSingleton<INotifier, FakeNotifier>();
            //services.AddSingleton<IPaymentManager, FakePaymentManager>();


            //services.AddSingleton<IWalletCreator, FakeWalletCreator>();
            //services.AddSingleton<IImageManager, FakeImageManager>();
            //services.AddSingleton<IImageManager, AzureDocumentManager>();
            //services.AddSingleton<IWalletService, FakeWalletService>();
            //services.AddSingleton<ICryptoService, FakeCryptoService>();
            //services.AddSingleton<IInterWalletService, FakeInterWalletService>();
            //--Utilities
            services.AddSingleton<IMessageProvider, MessageProvider>();
            services.AddScoped<ICodeGenerator, CodeGenerator>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<ICryptoService, CryptographyService>();
            services.AddScoped<ITransactionTracker, TransactionTracker>();
            services.AddScoped<ITransactionTracker, TransactionTracker>();
            services.AddScoped<IConnectionConfigurations, ConnectionConfigurations>();
            services.AddScoped<IConnectionConfigurationCommitRoll, ConnectionConfigurationCommitRoll>();
            services.AddScoped<Connections>();

            //--Service layer
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IBenefitiariesDAO, BenefitiariesDAO>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ISubsidiaryTransferService, SubsidiaryTransferService>();
            services.AddScoped<INationalTransferService, NationalTransferService>();
            services.AddScoped<IAuthenticationServices, AuthenticationService>();
            services.AddScoped<IUserActivityService, UserActivityService>();
            services.AddScoped<ITransferBeneficiaryService, TransferBeneficiaryService>();
            services.AddTransient<IPaymentBeneficiaryService, PaymentBeneficiaryService>();
            services.AddScoped<IWalletOpeningService, WalletOpeningService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IBillsService, BillsService>();
            services.AddScoped<IInterWalletService, InterWalletService>();
            services.AddScoped<ICustomerDocumentService, CustomerDocumentService>();
            services.AddScoped<IAccountOpeningService, AccountOpeningService>();
            //  services.AddScoped<IAccountOpeningService, AccountOpeningService>();




            //---Concrete implementations
            services.AddScoped<IBeneficiaryService, BeneficiaryService>();
            services.AddScoped<ILocalAccountService, LocalClient>();
            services.AddScoped<IExternalTransferService, ExternalClient>();
            services.AddSingleton<ISubsidiaryService, FGTClient>();
            services.AddScoped<IAuthenticator, Authenticator>();
            services.AddScoped<IReversalService, ReversalService>();

            services.AddScoped<ICustomerDAO, CustomerDAO>();
            services.AddScoped<ISessionDAO, SessionDAO>();
            services.AddScoped<IBillerDAO, BillerDAO>();
            services.AddScoped<IProductDAO, ProductDAO>();
            services.AddScoped<IInstitutionDAO, InstitutionDAO>();
            services.AddScoped<IDeviceDAO, DeviceDAO>();
            services.AddScoped<IReversalDAO, ReversalDAO>();
            services.AddScoped<IReversalLogDAO, ReversalLogDAO>();
            services.AddScoped<ILimitDAO, LimitDAO>();
            services.AddScoped<Core.DAO.ITransactionDAO, Core.DAO.TransactionDAO>();
            services.AddScoped<IOtpDAO, OtpDAO>();
            services.AddScoped<IUserActivityDAO, UserActivityDAO>();
            services.AddScoped<IWalletOpeningRequestDAO, WalletOpeningRequestDAO>();
            services.AddScoped<IDeviceHistoryDAO, DeviceHistoryDAO>();
            services.AddScoped<ILimitService, LimitService>();
            services.AddScoped<IUnitOfWorkSession, UnitOfWorkSession>();
            services.AddScoped<IUserActivityDAO, UserActivityDAO>();
            services.AddScoped<ITransactionTrackerDAO, TransactionTrackerDAO>();
            services.AddScoped<ICaseDAO, CaseDAO>();
            services.AddScoped<IDocumentDAO, DocumentDAO>();
            services.AddScoped<IDashboardIntegrationService, DashboardIntegrationService>();
            services.AddScoped<ITransactionNotificationService, TransactionNotificationService>();
            services.AddScoped<ITransactionNotificationDAO, TransactionNotificationDAO>();

            services.AddSingleton<IProfileManager, ProfileManager>();
            services.AddSingleton<IImageManager, FileSystemImageManager>();
            services.AddScoped<IWalletCreator, WalletService>();
            services.AddScoped<IWalletService, WalletService>();

            services.AddSingleton<ILanguageConfigurationProvider, FileSystemLanguageProvider>();

            //services.AddScoped<INotifier, SMSService>();
            //services.AddScoped<IPaymentManager, PaymentManager>();

            services.AddTransient<HttpClient>();

            //--Filters
            services.AddScoped<SessionFilter>();
            // services.AddScoped<UserActivityFilter>();
            services.AddSingleton<LanguageFilter>();
            services.AddScoped<DuplicateFilter>();
            services.AddScoped<APIKeyFilter>();
            services.AddScoped<ValdateTokenFilter>();


            var connection = Configuration.GetConnectionString("ApplicationConnection");
            services.AddScoped<IDbConnection>((s) => new SqlConnection(connection));

            services.Configure<OnboardingConfigurationProvider>(opt => Configuration.GetSection("OnboardingConfigurations").Bind(opt));
            services.Configure<FISettings>(opt => Configuration.GetSection("FISettings").Bind(opt));
            services.Configure<WalletConfigSettings>(opt => Configuration.GetSection("WalletConfigSettings").Bind(opt));
            services.Configure<SystemSettings>(opt => Configuration.GetSection("SystemSettings").Bind(opt));
            services.Configure<StatementServiceSettings>(opt => Configuration.GetSection("StatementServiceSettings").Bind(opt));
            services.Configure<BeneficiarySettings>(opt => Configuration.GetSection("BeneficiarySettings").Bind(opt));
            services.Configure<InterWalletTransferSettings>(opt => Configuration.GetSection("InterWalletTransferSettings").Bind(opt));
            services.Configure<SMSProviderSetting>(opt => Configuration.GetSection("SmsProvider").Bind(opt));
            services.Configure<BillsPaySettings>(opt => Configuration.GetSection("BillsPaySettings").Bind(opt));
            services.Configure<LanguageSettings>(opt => Configuration.GetSection("LanguageSettings").Bind(opt));
            services.Configure<AccountOpeningSettings>(opt => Configuration.GetSection("AccountOpeningSettings").Bind(opt));
            services.Configure<SchemeCodeForAccountChanges>(opt => Configuration.GetSection("AccountChanges").Bind(opt));
            services.Configure<SenegalExternalCallSetting>(opt => Configuration.GetSection("SenegalExternalCallSetting").Bind(opt));
            //services.Configure<LanguageSettings>(opt => Configuration.GetSection("LanguageSettings").Bind(opt));            
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

            services.AddHttpClient<IPaymentManager, PaymentManager>().ConfigurePrimaryHttpMessageHandler(() =>
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
            services.AddHttpClient<IPaymentManager, PaymentManager>().ConfigurePrimaryHttpMessageHandler(() =>
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

            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "FBNMobile Senegal API", Version = "v1" });
            //});


            services.AddSwaggerGen(c =>
            {

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "FBNMobile Senegal V3 API",
                    Description = "This API provides all the endpoints needed to extablish the FBNMobile Senegal calls",
                    // TermsOfService = new Uri(""),
                    Contact = new OpenApiContact
                    {
                        Name = "First Bank  Senegal",
                        Email = string.Empty,
                        //  Url = new Uri(" "),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under Firstbank Senegal",
                        //  Url = new Uri(""),
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(System.AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);
            });
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            var path = Configuration.GetSection("SystemSettings:LogingPath");
            var pathValue = path?.Value;
            if (!string.IsNullOrWhiteSpace(pathValue))
            {
                if (!Directory.Exists(pathValue))
                {
                    Directory.CreateDirectory(pathValue);

                }
                // var fullFilePath = Path.Combine(pathValue, "senegalnlog-all-{Date}.txt");
                var fullFilePath = pathValue + Path.DirectorySeparatorChar + "SenegalerologV3-all-{Date}.txt";

                loggerFactory.AddFile(fullFilePath);
            }


            // loggerFactory.AddFile("C:/SenegalMiddleware_Logs/Logs/fbnmobile/senegalnlog-all-{Date}.txt");





            // loggerFactory.AddFile("C:/Logs/fbnmobile/nlog-all-{Date}.txt");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseConfigureExceptionHandler(loggerFactory);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";
                c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "FBNMobile Senegal V3 API");
            });
   // app.UseHttpsRedirection();

 

            app.UseRouting();

            app.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials());

            app.UseStaticFiles();



            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
