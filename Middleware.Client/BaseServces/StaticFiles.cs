using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Middleware.Core.DTO;
using Middleware.Core.Model;
using Middleware.Service;
using Middleware.Service.BAP;
using Middleware.Service.Beneficiary;
using Middleware.Service.FIServices;
using Middleware.Service.InterWalletServices;
using Middleware.Service.Onboarding;
using Middleware.Service.SmsService;
using Middleware.Service.Utilities;
using Middleware.Service.WalletServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Client.BaseServces
{
    public static class StaticFiles
    {
     
        public static IServiceCollection AddStaticFiles(this IServiceCollection services, IConfiguration Configuration)
        {
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

            return services;
        }
    }
}
