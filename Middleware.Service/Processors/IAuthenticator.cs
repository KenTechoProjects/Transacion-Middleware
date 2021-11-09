using System;
using System.Threading.Tasks;
using Middleware.Service.DTOs;
using Middleware.Service.Onboarding;

namespace Middleware.Service.Processors
{
    public interface IAuthenticator
    {
        Task<ServiceResponse<ServiceAuthenticationResponse>> AuthenticateWithEmail(string emailAddress, string password);
        Task<ServiceResponse<ServiceAuthenticationResponse>> AuthenticateWithPhone(string phoneNumber, string password);
        Task<BasicResponse> ValidatePin(string customerId, string pin);
        Task<BasicResponse> ChangePassword(string customerId, string oldPassword, string newPassword);
        Task<BasicResponse> ChangePin(string customerId, string oldPin, string newPin);
        Task<BasicResponse> ResetPassword(ResetPasswordRequest reset, string customerId);
        Task<BasicResponse> ResetPin(ResetPinRequest reset, string customerId);
        Task<BasicResponse> ResetQuestions(ResetQuestionsRequest request);
        Task<ServiceResponse<SecurityQuestion>> GetSecurityQuestion(string customerId);
        Task<BasicResponse> ValidateAnswer(string customerId, Answer answer);
        Task<BasicResponse> SendOtp(string customerId, OtpTargetFeature otpTargetFeature);

    }
}
