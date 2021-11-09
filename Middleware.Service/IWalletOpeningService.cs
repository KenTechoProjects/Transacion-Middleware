using System;
using System.Threading.Tasks;
using Middleware.Service.DTOs;

namespace Middleware.Service
{
    public interface IWalletOpeningService
    {
        object SenegalCode();
        Task<string> GetUniqueReferralCode();
       
        Task<ServiceResponse<WalletInitialisationResponse>> InitialiseWallet(WalletInitialisationRequest request, string language);
        Task<BasicResponse> AddIdentificationDocument(IdUpdateRequest request, string language);
        Task<ServiceResponse<PhotoUploadResponse>> AddPhoto(PhotoUpdateRequest request, string language);
        Task<BasicResponse> SendValidationCode(string phoneNumber, string language);
        Task<ServiceResponse<WalletCompletionResponse>> Complete(WalletCompletionRequest request, string language);
        Task<ServiceResponse<WalletStatus>> GetWalletOpeningStatus(string phoneNumber, string language);
        Task<BasicResponse> UpdatePersonalInformation(string phoneNumber, Biodata data, string language);
        Task<BasicResponse> CreateOTP(string phoneNumber, string language);
        Task<BasicResponse> PhoneNumberAlreadyOnboarded(string phoneNumber);
        Task<ReferralCodeResponse> ReferralCodeExists(string referralCode,string language);


    }
}
