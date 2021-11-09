using Middleware.Core.Model;
using Middleware.Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Middleware.Service.Processors
{
    public interface IInterWalletService
    {
        Task<ServiceResponse<IEnumerable<WalletSchemes>>> GetSchemes();
        Task<ServiceResponse<string>> GetWalletName(string walletId, string walletScheme);
        Task<BasicResponse> GetTransactionStatus(string transactionReference);
        //Task<ServiceResponse<InterWalletTransferResponse>> Transfer(InterWalletTransferRequest request, AuthenticatedUser user, string language);
        Task<ServiceResponse<TransferResponse>> Transfer(AuthenticatedTransferRequest request, string transactionReference, string language);

        
        Task<ServiceResponse<IEnumerable<WalletSchemes>>> GetWalletSchemes();
        Task<ServiceResponse<string>> DoWalletNameValidation(string walletId, string walletScheme);
        Task<ServiceResponse<ServiceChargeResponse>> GetServiceCharge(decimal amount, decimal balance);
    }
}
