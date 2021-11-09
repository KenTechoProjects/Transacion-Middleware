using Middleware.Core.DTO;
using Middleware.Core.Model;
using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountOpeningRequest = Middleware.Core.DTO.AccountOpeningRequest;

namespace Middleware.Service.Processors
{
    public interface IAccountOpeningService
    {
        Task<ServiceResponse<AccountOpeningResponse>> OpenAccountWithWallet(AccountOpeningRequestForCustomerWithWallet request, string language);
        Task<ServiceResponse<AccountOpeningResponse>> OpenAccountWithNoWallet(AccountOpeningCompositRequest request, string language);
        Task<BasicResponse> RemovePND(PNDRemovalRequest request);
        Task<BasicResponse> HasWallet(string walletNumber);
        Task<BasicResponse> HasAcount(string accountNumber);

        Task<Customer> Getwallet(string walletNumber);
        Task<ServiceResponse<AccountOpeningResponse>> AccountOnboarding(/*AccountOpeningOnboardingInitiationRequest*/ dynamic request, string language, bool hasWallet = false);
    }
}
