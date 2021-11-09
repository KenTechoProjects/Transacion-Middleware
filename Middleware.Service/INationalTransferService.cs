using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Service.DTOs;
using Middleware.Service.InterWalletServices;

namespace Middleware.Service
{
    public interface INationalTransferService
    {
        Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber, string institutionCode, string language);
        Task<ServiceResponse<string>> GetWalletName(string walletId, string walletScheme, string language);
        Task<ServiceResponseT<TransferResponse>> Transfer(AuthenticatedTransferRequest request, AuthenticatedUser user, string language, bool saveAsBeneficiary);
        Task<ServiceResponse<TransferResponse>> TransferToSelf(SelfTransferRequest request, AuthenticatedUser user, string language);
        Task<InstitutionResult> GetInstitutions();
        Task<ServiceResponse<IEnumerable<Branch>>> GetBankBranches(string bankCode, string language);
        Task<ServiceResponse<TransferResponse>> WalletToAccount(AuthenticatedTransferRequest request, AuthenticatedUser user, string language, bool saveAsBeneficiary);
        Task<ServiceResponse<TransferResponse>> WalletToWallet(AuthenticatedTransferRequest request, AuthenticatedUser user, string language, bool saveAsBeneficiary);
        Task<ServiceResponseT<TransferResponse>> AccountToWallet(AuthenticatedTransferRequest request, AuthenticatedUser user, string language, bool saveAsBeneficiary);
        Task<ServiceResponse<TransferResponse>> WalletToSelf(SelfTransferRequest request, AuthenticatedUser user, string language);
        Task<ServiceResponse<IEnumerable<WalletSchemes>>> GetWalletSchemes(string language);
        Task<ServiceResponse<ServiceChargeResponse>> GetTransferCharge(decimal amount, decimal balance, string language);

    }
}