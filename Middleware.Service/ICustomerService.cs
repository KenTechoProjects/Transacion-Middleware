using System;
using System.Threading.Tasks;
using Middleware.Core.Model;
using Middleware.Service.DTOs;
using Middleware.Service.Onboarding;

namespace Middleware.Service
{
    public interface ICustomerService
    {
        Task<BasicResponse> ChangePassword(ChangePasswordRequest request, string customerId, string language);
        Task<BasicResponse> ChangePin(ChangePinRequest request, string customerId, string language);
        Task<BasicResponse> ResetPin(ResetPinRequest request, string customerId, string language);
        Task<BasicResponse> ResetPassword(ResetPasswordRequest request, string language);
        Task<BasicResponse> ResetQuestion(ResetQuestionsRequest request, string language);
        Task<ServiceResponse<SecurityQuestion>> GetSecurityQuestion(string customerId, string language);
        Task<BasicResponse> SendOtp(OtpRequest request, string language);
        Task<BasicResponse> SendOtpNew(OtpRequest request, string language);
 
        Task<ServiceResponse<string>> GetSelfie(string walletNumber, string language);
        Task<BasicResponse> InitiatePasswordReset(InitiatePasswordResetRequest request, string language);
    }
}
