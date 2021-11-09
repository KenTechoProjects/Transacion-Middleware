using Microsoft.Extensions.DependencyInjection;
using Middleware.Client.Filters;
using Middleware.Core.DAO;
using Middleware.Service;
using Middleware.Service.Beneficiary;
using Middleware.Service.DAO;
using Middleware.Service.Fakes;
using Middleware.Service.FIServices;
using Middleware.Service.Implementations;
using Middleware.Service.InterWalletServices;
using Middleware.Service.Onboarding;
using Middleware.Service.Processors;
using Middleware.Service.TwofactorAuthentications;
using Middleware.Service.Utilities;
using Middleware.Service.WalletServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Middleware.Client.BaseServces
{
    public static class ServiceBase
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {

             services.AddSingleton<INotifier, FakeNotifier>();
            services.AddSingleton<IPaymentManager, FakePaymentManager>();
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
            services.AddScoped<ITwofactorAuthentication, TwofactorAuthentication>();
            services.AddScoped<ICurrencyCode, CurrencyCode>();
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
            //Fake
            return services;
        }

  public static IServiceCollection AddFakeServices(this IServiceCollection services)
        {

          
        //    services.AddSingleton<IAuthenticator, FakeAuthenticator>();

        //    services.AddSingleton<ICustomerDAO, FakeDAO>();
        //    services.AddSingleton<ISessionDAO, FakeDAO>();
        //    services.AddSingleton<IInstitutionDAO, FakeDAO>();
        //    services.AddSingleton<IDeviceDAO, FakeDAO>();
        //    services.AddSingleton<IUserActivityDAO, FakeDAO>();
        //    services.AddSingleton<ILimitDAO, FakeDAO>();
        ////    services.AddSingleton<ITransactionDAO, FakeDAO>();
        //    services.AddSingleton<ITransactionTrackerDAO, FakeDAO>();
        //    services.AddSingleton<IWalletOpeningRequestDAO, FakeDAO>();


        //    services.AddSingleton<ILocalAccountService, FakeBankService>();
        //    services.AddSingleton<IBeneficiaryService, FakeBeneficiaryService>();
        //    services.AddSingleton<ILimitService, FakeLimitService>();
        //    services.AddScoped<IUnitOfWorkSession, FakeUnitOfWorkSession>();


        //    services.AddSingleton<ISubsidiaryService, FakeSubsidiaryService>();
        //    services.AddSingleton<IExternalTransferService, FakeInterBankService>();

        //   services.AddSingleton<ILanguageConfigurationProvider, FakeLanguageProvider>();

        //    services.AddSingleton<IProfileManager, FakeProfileManager>();


        //    services.AddSingleton<INotifier, FakeNotifier>();
        //    services.AddSingleton<IPaymentManager, FakePaymentManager>();

        //    services.AddSingleton<IWalletCreator, FakeWalletCreator>();
        //  //  services.AddSingleton<IImageManager, FakeImageManager>();
        //   // services.AddSingleton<IImageManager, AzureDocumentManager>();
        //    services.AddSingleton<IWalletService, FakeWalletService>();
        //    services.AddSingleton<ICryptoService, FakeCryptoService>();
            //services.AddSingleton<IInterWalletService, FakeInterWalletService>();
            return services;
        }

    }
}
