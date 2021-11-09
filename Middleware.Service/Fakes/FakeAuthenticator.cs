using System;
using System.Threading.Tasks;
using Middleware.Service.DTOs;
using Middleware.Service.Onboarding;
using Middleware.Service.Processors;

namespace Middleware.Service.Fakes
{
    public class FakeAuthenticator : IAuthenticator
    {
        public Task<ServiceResponse<ServiceAuthenticationResponse>> AuthenticateWithEmail(string emailAddress, string password)
        {
            var payload = new ServiceAuthenticationResponse
            {
                FirstName = "Test",
                MiddleName = "FBN",
                LastName = "User",
                BankId = "B001",
                WalletId = "W001"
            };
            var response = new ServiceResponse<ServiceAuthenticationResponse>(true);
            response.SetPayload(payload);
            return Task.FromResult(response);
        }
        public Task<BasicResponse> ValidatePin(string cif, string pin)
        {
            return Task.FromResult(new BasicResponse(true));
        }
        public Task<BasicResponse> ChangePassword(string customerId, string oldPassword, string newPassword)
        {
            return Task.FromResult(new BasicResponse(true));
        }

        public Task<BasicResponse> ChangePin(string customerId, string oldpin, string newpin)
        {
            return Task.FromResult(new BasicResponse(true));
        }

        public Task<ServiceResponse<SecurityQuestion>> GetSecurityQuestion(string customerId)
        {
            var response = new ServiceResponse<SecurityQuestion>(true);
            response.SetPayload(new SecurityQuestion { QuestionValue = "What is your mother's maiden name?", QuestionId = 1 });
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<OnboardingInitiationResponse>>  InitiateOnboarding(OnboardingInitiationRequest request)
        {
            var response = new ServiceResponse<OnboardingInitiationResponse>(true);
            response.SetPayload(new OnboardingInitiationResponse
            {
                AccountNumber = request.AccountNumber,
                CifId = "23479",
                Email = "testc@firstbanknigeria.com",
                FirstName = "John",
                LastName = "Doe",
                MiddleName = "Simpson",
                PhoneNumber = "08029091811"
            });
            return Task.FromResult(response);
        }

        public Task<BasicResponse> CompleteOnboarding(OnboardingCompletionRequest request, string customerId)
        {
            var response = new BasicResponse(true);
            return Task.FromResult(response);
        }

        public Task<BasicResponse> ValidateAnswer(string customerId, Answer answer)
        {
            var response = new BasicResponse(false);
            if(answer.Response == "digital")
            {
                response.IsSuccessful = true;           
            }
            else
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INCORRECT_SECURITY_ANSWER
                };
            }
            return Task.FromResult(response);
        }


        public Task<BasicResponse> ResetPassword(ResetPasswordRequest reset, string customerId)
        {
            return Task.FromResult(new BasicResponse(true));
        }
        public Task<BasicResponse> ResetPin(ResetPinRequest reset, string customerId)
        {
            return Task.FromResult(new BasicResponse(true));
        }

        public Task<BasicResponse> SendOtp(string customerId, OtpTargetFeature otpTargetFeature)
        {
            return Task.FromResult(new BasicResponse(true));

        }

        public Task<BasicResponse> ResetQuestions(ResetQuestionsRequest request)
        {
            return Task.FromResult(new BasicResponse(true));
        }

        public Task<ServiceResponse<ServiceAuthenticationResponse>> AuthenticateWithPhone(string phoneNumber, string password)
        {
            var payload = new ServiceAuthenticationResponse
            {
                FirstName = "Test",
                MiddleName = "FBN",
                LastName = "User",
                BankId = "B001",
                WalletId = "W001"
            };
            var response = new ServiceResponse<ServiceAuthenticationResponse>(true);
            response.SetPayload(payload);
            return Task.FromResult(response);
        }
    }
}
