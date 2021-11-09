using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.Onboarding
{
    public class Authenticator : IAuthenticator
    {
        readonly ILogger _logger;
        private const string _prefix = "ONB";
        private readonly HttpClient _client;

        private readonly OnboardingEndpoints _endpoints;


        public Authenticator(IOptions<OnboardingConfigurationProvider> onboadingConfigProvider, ILoggerFactory logger, IHttpClientFactory factory)
        {
            _endpoints = onboadingConfigProvider.Value.OnboardingEndpoints;
            _logger = logger.CreateLogger(typeof(Authenticator));
            _client = factory.CreateClient("HttpMessageHandler");
            _client.BaseAddress = new Uri(onboadingConfigProvider.Value.BaseUrl);
            _client.DefaultRequestHeaders.Add("AppId", onboadingConfigProvider.Value.AppId);
            _client.DefaultRequestHeaders.Add("AppKey", onboadingConfigProvider.Value.AppKey);

        }


        public async Task<ServiceResponse<ServiceAuthenticationResponse>> AuthenticateWithEmail(string email, string password)
        {
            var response = await PostRequest<ServiceAuthenticationResponse, AuthenticationRequest>(new AuthenticationRequest
            {
                Email = email,
                Password = password
            },
           _endpoints.Auth_Email);
            // "auth/email") ;

            _logger.LogInformation("AuthenticateWithEmail  response:===>{0}", JsonConvert.SerializeObject(response));
            return response;
        }

        public async Task<ServiceResponse<ServiceAuthenticationResponse>> AuthenticateWithPhone(string phoneNumber, string password)
        {

            var response = await PostRequest<ServiceAuthenticationResponse, AuthenticationRequest>(new AuthenticationRequest
            {
                PhoneNumber = phoneNumber,
                Password = password
            },
                      _endpoints.Auth_Phone);
            // "auth/phone");
            _logger.LogInformation(" AuthenticateWithPhone  response:===>{0}", JsonConvert.SerializeObject(response));
            return response;


        }

        public async Task<BasicResponse> ChangePassword(string username, string oldPassword, string newPassword)
        {
            var data = await PostRequest<object>(new
            {
                phoneNumber = username,
                OldPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = newPassword

            },
          _endpoints.Change_Password);
            //"customer/password");
            _logger.LogInformation(" ChangePassword  response:===>{0}", JsonConvert.SerializeObject(data));

            return data;
        }

        public async Task<BasicResponse> ChangePin(string username, string oldpin, string newpin)
        {
            var data = await PostRequest<object>(new
            {
                phoneNumber = username,
                oldpin,
                newpin,
                confirmNewpin = newpin

            },
            _endpoints.Pin_Change);
            //"customer/pin");

            _logger.LogInformation(" ChangePin   response:===>{0}", JsonConvert.SerializeObject(data));

            return data;
        }

        //Chck on this later
        public async Task<ServiceResponse<SecurityQuestion>> GetSecurityQuestion(string username)
        {
            var data = await GetRequest<SecurityQuestion>($"customer/{username}/questions");
            _logger.LogInformation(" GetSecurityQuestion   response:===>{0}", JsonConvert.SerializeObject(data));
            return data;
        }



        public async Task<BasicResponse> ResetPassword(ResetPasswordRequest request, string customerId)
        {
            var data = await PostRequest<object>(new
            {
                phoneNumber = request.UserName,
                QuestionId = request.Answer.QuestionID,
                Answer = request.Answer.Response,
                Password = request.NewPassword,
                ConfirmPassword = request.ConfirmNewPassword
            },
            _endpoints.Reset_Password);
            //"customer/password");
            _logger.LogInformation(" ResetPassword   Request:===>{0}", JsonConvert.SerializeObject(request));

            _logger.LogInformation(" ResetPassword   response:===>{0}", JsonConvert.SerializeObject(data));
            return data;
        }

        public async Task<BasicResponse> ResetQuestions(ResetQuestionsRequest request)
        {
            var data = await PostRequest<object>(new
            {
                phoneNumber = request.Username,
                otp = request.Otp,
                securityQuestions = request.SecretQuestions
            },
           _endpoints.Question_Change);
            //"customer/questions-without-otp");
            _logger.LogInformation(" ResetQuestions   Request:===>{0}", JsonConvert.SerializeObject(request));
            _logger.LogInformation("ResetQuestion response:===>{0}", JsonConvert.SerializeObject(data));


            return data;
        }

        public async Task<BasicResponse> ResetPin(ResetPinRequest request, string username)
        {


            var data = await PostRequest<object>(new
            {
                phoneNumber = username,
                QuestionId = request.Answer.QuestionID,
                Answer = request.Answer.Response,
                Pin = request.NewPin,
                ConfirmPin = request.ConfirmNewPin
            },

           _endpoints.Pin_Reset);
            _logger.LogInformation(" ResetPin  Request:===>{0}", JsonConvert.SerializeObject(request));
            //"customer/pin");
            _logger.LogInformation(" ResetPin   response:===>{0}", JsonConvert.SerializeObject(data));

            return data;
        }

        public async Task<BasicResponse> SendOtp(string username, OtpTargetFeature otpTargetFeature)
        {
            var data = await PostRequest<object>(new
            {
                phoneNumber = username,
                otpTargetFeature = otpTargetFeature.ToString()
            },
           _endpoints.Send_OTP);
            //"customer/otp");
            _logger.LogInformation(" SendOtp   response:===>{0}", JsonConvert.SerializeObject(data));
            return data;
        }

        public async Task<BasicResponse> ValidateAnswer(string customerId, Answer answer)
        {
            _logger.LogInformation("ValidateAnswer Request:===>{0} ", JsonConvert.SerializeObject(new { customerId, answer }));
            var data = await PostRequest<object>(new
            {
                answer.QuestionID,
                answer = answer.Response
            },
           _endpoints.Validate_Answer);
            //"customer/question/validate");
            _logger.LogInformation("ValidateAnswer Response:===>{0} ", JsonConvert.SerializeObject(data));


            if (data.Error?.ResponseCode == "ONB")
            {
                data.Error.ResponseCode = ResponseCodes.INCORRECT_SECURITY_ANSWER;

            }
            return data;
        }


        public async Task<BasicResponse> ValidatePin(string customerId, string pin)
        {
            _logger.LogInformation("Inside the ValidatePin method of the Authenticator");
            _logger.LogInformation("ValidatePin method of the Authenticator. Request:===>{0}", JsonConvert.SerializeObject(new { customerId, pin }));
            var data = await PostRequest<object>(new { PhoneNumber = customerId, pin },
               _endpoints.Validate_Pin);
            //"auth/pin");
            _logger.LogInformation("Response from ValidatePin method of the Authenticator. Response:===>{0}", JsonConvert.SerializeObject(data));
            return data;
        }


        private async Task<ServiceResponse<T>> PostRequest<T, U>(U request, string path)
        {
            var response = new ServiceResponse<T>(false);

            var input = Util.SerializeAsJson(request);
            var message = new StringContent(input, Encoding.UTF8, "application/json");

            var rawResponse = await _client.PostAsync(path, message);
           // _logger.LogInformation("Raw response {response}", JsonConvert.SerializeObject(rawResponse));
            var body = await rawResponse.Content.ReadAsStringAsync();
            //_logger.LogInformation($"BODY: {body}");
            if (rawResponse.IsSuccessStatusCode)
            {
                response.IsSuccessful = true;
                if (!string.IsNullOrEmpty(body))
                {
                    var data = Util.DeserializeFromJson<T>(body);
                    response.SetPayload(data);
                }
            }
            else
            {

                var error = Util.DeserializeFromJson<OnboardingError>(body);
                response.Error = new ErrorResponse { ResponseCode = $"{_prefix}{error?.Code}", ResponseDescription = error?.Message };


            }
            return response;
        }

        private async Task<BasicResponse> PostRequest<U>(U request, string path)
        {
            var response = new BasicResponse(false);

            var input = Util.SerializeAsJson(request);
            var message = new StringContent(input, Encoding.UTF8, "application/json");

            var rawResponse = await _client.PostAsync(path, message);

            var body = await rawResponse.Content.ReadAsStringAsync();
            if (rawResponse.IsSuccessStatusCode)
            {
                response.IsSuccessful = true;
            }
            else
            {
                var error = Util.DeserializeFromJson<OnboardingError>(body);
                response.Error = new ErrorResponse { ResponseCode = $"{_prefix}{error?.Code}", ResponseDescription = error?.Message };
            }
            return response;
        }

        private async Task<ServiceResponse<T>> PutRequest<T, U>(U request, string path)
        {
            var response = new ServiceResponse<T>(false);

            var input = Util.SerializeAsJson(request);
            var message = new StringContent(input, Encoding.UTF8, "application/json");

            var rawResponse = await _client.PutAsync(path, message);

            var body = await rawResponse.Content.ReadAsStringAsync();
            if (rawResponse.IsSuccessStatusCode)
            {
                response.IsSuccessful = true;
                if (!string.IsNullOrEmpty(body))
                {
                    var data = Util.DeserializeFromJson<T>(body);
                    response.SetPayload(data);
                }

            }
            else
            {
                var error = Util.DeserializeFromJson<OnboardingError>(body);
                response.Error = new ErrorResponse { ResponseCode = $"{_prefix}{error.Code}", ResponseDescription = error.Message };
            }
            return response;
        }

        private async Task<BasicResponse> PutRequest<U>(U request, string path)
        {
            var response = new BasicResponse(false);

            var input = Util.SerializeAsJson(request);
            var message = new StringContent(input, Encoding.UTF8, "application/json");
            var rawResponse = await _client.PutAsync(path, message);

            var body = await rawResponse.Content.ReadAsStringAsync();
            if (rawResponse.IsSuccessStatusCode)
            {
                response.IsSuccessful = true;
            }
            else
            {
                var error = Util.DeserializeFromJson<OnboardingError>(body);
                response.Error = new ErrorResponse { ResponseCode = $"{_prefix}{error.Code}", ResponseDescription = error.Message };
            }
            return response;
        }

        private async Task<ServiceResponse<T>> GetRequest<T>(string path)
        {
            var response = new ServiceResponse<T>(false);
            var rawResponse = await _client.GetAsync(path);
            var body = await rawResponse.Content.ReadAsStringAsync();
            if (rawResponse.IsSuccessStatusCode)
            {
                var data = Util.DeserializeFromJson<T>(body);
                response.IsSuccessful = true;
                response.SetPayload(data);
            }
            else
            {
                var error = Util.DeserializeFromJson<OnboardingError>(body);
                response.Error = new ErrorResponse { ResponseCode = $"{_prefix}{error?.Code}", ResponseDescription = error?.Message };
            }
            return response;
        }


    }
}
