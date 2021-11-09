using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Core.DAO;
using Middleware.Core.Model;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.Model;
using Middleware.Service.Onboarding;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.Implementations
{
    public class CustomerService : ICustomerService
    {
        readonly IAuthenticator _authenticator;
        readonly IMessageProvider _messageProvider;
        readonly IDeviceDAO _deviceDAO;
        readonly ICustomerDAO _customerDAO;
        private readonly IOtpDAO _otpDAO;
        private readonly ICryptoService _cryptoService;
        readonly ILogger _logger;
        readonly SystemSettings _settings;
        private readonly IOtpService _otpService;
        private readonly INotifier _notifier;

        public CustomerService(IAuthenticator authenticator, IMessageProvider messageProvider, IOtpDAO otpDAO, ICryptoService cryptoService,
            IDeviceDAO deviceDAO, ICustomerDAO customerDAO, ILoggerFactory logger, IOptions<SystemSettings> settingsProvider, IOtpService otpService, INotifier notifier)
        {
            _authenticator = authenticator;
            _messageProvider = messageProvider;
            _deviceDAO = deviceDAO;
            _customerDAO = customerDAO;
            _logger = logger.CreateLogger(typeof(CustomerService));
            _settings = settingsProvider.Value;
            _otpDAO = otpDAO;
            _cryptoService = cryptoService;
            _otpService = otpService;
            _notifier = notifier;
        }

        public async Task<BasicResponse> ChangePassword(ChangePasswordRequest request, string username, string language)
        {
            ///TODO: Do checks here before calling authenticator
            var response = new BasicResponse(false);

            if (!request.NewPassword.Equals(request.ConfirmNewPassword))
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INPUT_VALIDATION_FAILURE, language)
                };
                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                return response;
            }

            response = await _authenticator.ChangePassword(username, request.OldPassword, request.NewPassword);

            if (!response.IsSuccessful)
            {
                if (response.Error?.ResponseCode == "ONB")
                {
                    
                    response.Error.ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER; 
                    response.Error.ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER , language);
                }
                else
                {
 response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                }
               
                //TODO: Determine fault type
            }

            return response;
        }

        public async Task<BasicResponse> ChangePin(ChangePinRequest request, string username, string language)
        {
            var response = new BasicResponse(false);

            if (!request.NewPin.Equals(request.ConfirmNewPin))
            {
                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                response.IsSuccessful = false;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)}"
                };
                return response;
            }
 
           
            response = await _authenticator.ChangePin(username, request.OldPin, request.NewPin);
            if (!response.IsSuccessful)
            {
                if (response.Error?.ResponseCode == "ONB")
                {
                    response.Error.ResponseCode = ResponseCodes.INVALID_TRANSACTION_PIN; 
                    response.Error.ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_TRANSACTION_PIN , language);
                }
                else
                {
    response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                }

            
                //TODO: Determine fault type
            }
            return response;
        }

        public async Task<ServiceResponse<string>> GetSelfie(string walletNumber, string language)
        {
            var response = new ServiceResponse<string>(false);

            var selfieUrl = $"{_settings.SelfieBaseUrl}{walletNumber}.{_settings.ImageFormat}";
            response.IsSuccessful = true;

            response.SetPayload(selfieUrl);
            return await Task.FromResult(response);
        }

        public async Task<BasicResponse> ResetPin(ResetPinRequest request, string username, string language)
        {
            var response = new BasicResponse(false);

            if (!request.NewPin.Equals(request.ConfirmNewPin))
            {
                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                response.IsSuccessful = false;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)}"
                };
                return response;
            }



            response = await _authenticator.ResetPin(request, username);

            if (!response.IsSuccessful)
            {
                if (response.Error?.ResponseCode == "ONB")
                {
                    response.Error.ResponseCode = ResponseCodes.INVALID_TRANSACTION_PIN; 
                    response.Error.ResponseDescription = _messageProvider.GetMessage( ResponseCodes.INVALID_TRANSACTION_PIN , language);
                }
                else
                {
  response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                }
              
                //TODO: Determine fault type
            }
            return response;
        }

        public async Task<BasicResponse> ResetPassword(ResetPasswordRequest request, string language)
        {
            _logger.LogInformation("Inside the ResetPassword of customer service");
            var response = new BasicResponse(false);
            request.UserName = request.UserName.Trim().ToLower();
            if (!request.NewPassword.Equals(request.ConfirmNewPassword))
            {
                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                response.IsSuccessful = false;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)}"
                };

                return response;
            }

            var customer = await _customerDAO.FindByWalletNumber(request.UserName);

            if (customer == null)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.CLIENT_INVALID_ARGUMENT,
                      ResponseCodes.CUSTOMER_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language));

            }

            if (!customer.IsActive)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.UNAUTHORIZED,
                       ResponseCodes.PROFILE_DEACTIVATED, _messageProvider.GetMessage(ResponseCodes.PROFILE_DEACTIVATED, language));
            }


            //var device = await _deviceDAO.Find(request.DeviceId);
            //if (device == null)
            //{
            //    return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.UNAUTHORIZED,
            //          ResponseCodes.DEVICE_MISMATCH, _messageProvider.GetMessage(ResponseCodes.DEVICE_MISMATCH, language));
            //}

            //if (device.Customer == null)
            //{
            //    return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.UNAUTHORIZED,
            //       ResponseCodes.DEVICE_MISMATCH, _messageProvider.GetMessage(ResponseCodes.DEVICE_MISMATCH, language));
            //}
            //if (device.Customer_Id != customer.Id)
            //{
            //    return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.UNAUTHORIZED,
            //       ResponseCodes.DEVICE_MISMATCH, _messageProvider.GetMessage(ResponseCodes.DEVICE_MISMATCH, language));
            //}
            //if (!device.IsActive)
            //{
            //    return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.UNAUTHORIZED,
            //        ResponseCodes.DEVICE_DISABLED, _messageProvider.GetMessage(ResponseCodes.DEVICE_DISABLED, language));
            //}


            var otp = await _otpDAO.Find(request.UserName, OtpPurpose.PASSWORD_RESET);
            if (otp == null)
            {
                return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.UNAUTHORIZED, ResponseCodes.CODE_VALIDATION_ERROR, _messageProvider.GetMessage(ResponseCodes.CODE_VALIDATION_ERROR, language));
            }


            if (DateTime.UtcNow > otp.DateCreated.AddMinutes(_settings.OtpDuration))
            {
                return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.UNAUTHORIZED, ResponseCodes.CODE_VALIDATION_ERROR,
                                                           _messageProvider.GetMessage(ResponseCodes.CODE_VALIDATION_ERROR, language));
            }



            if (!_cryptoService.AreEqual(request.Otp, otp.Code, otp.Salt))
            {
                return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.UNAUTHORIZED, ResponseCodes.CODE_VALIDATION_ERROR,
                    _messageProvider.GetMessage(ResponseCodes.CODE_VALIDATION_ERROR, language));
            }

            _logger.LogInformation("Start calling  _authenticator.ResetPassword method Inside the ResetPassword of customer service");
            response = await _authenticator.ResetPassword(request, customer.BankId);
            if (!response.IsSuccessful)
            {
                if (response.Error?.ResponseCode == "ONB")
                {
                    response.Error.ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER;
                    response.Error.ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER , language);
                }
                else
                {
 response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                }
               
                //TODO: Determine fault type
            }
            return response;
        }

        public async Task<ServiceResponse<SecurityQuestion>> GetSecurityQuestion(string customerId, string language)
        {
            var response = await _authenticator.GetSecurityQuestion(customerId);
            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
            }
            return response;
        }

        public async Task<BasicResponse> SendOtp(OtpRequest request, string language)
        {
            _logger.LogInformation("Inside the sendOtp of Customer Service");
            var response = new BasicResponse(false);

            if (!request.IsValid(out var source))
            {
                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}"
                };
                return response;
            }
            _logger.LogInformation("Calling _deviceDAO.Find Inside the sendOtp of Customer Service");
            var device = await _deviceDAO.Find(request.DeviceId);

            if (device == null)
            {
                _logger.LogInformation("Device is null qInside the sendOtp of Customer Service");
                response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                response.IsSuccessful = false;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.DEVICE_MISMATCH,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.DEVICE_MISMATCH, language)}"
                };

                return response;
            }



            var customer = device.Customer;

            if (customer.OnboardingStatus == OnboardingStatus.COMPLETED)
            {
                response.FaultType = FaultMode.INVALID_OBJECT_STATE;
                response.IsSuccessful = false;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.CUSTOMER_ALREADY_ONBOARDED,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.CUSTOMER_ALREADY_ONBOARDED, language)}"
                };

                return response;
            }
            //var usernameIsEmail = Util.IsEmail(request.UserName);

            var isOwnDevice = customer.WalletNumber == request.UserName;


            if (!isOwnDevice)
            {
                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                response.IsSuccessful = false;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.DEVICE_MISMATCH,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.DEVICE_MISMATCH, language)}"
                };

                return response;
            }
            response = await _authenticator.SendOtp(customer.BankId, OtpTargetFeature.CompleteOnboarding);

            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
            }

            return response;
        }

        public async Task<BasicResponse> SendOtpNew(OtpRequest request, string language)
        {
            var response = new BasicResponse(false);
            //var device = await _deviceDAO.Find(request.DeviceId);


            if (!request.IsValid(out var source))
            {
                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}"
                };
                return response;
            }

            Customer customer;

            var device = await _deviceDAO.Find(request.DeviceId);

            if (device == null)
            {
                response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                response.IsSuccessful = false;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.DEVICE_MISMATCH,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.DEVICE_MISMATCH, language)}"
                };

                return response;
            }

            customer = device.Customer;

            var isOwnDevice = customer.WalletNumber == request.UserName;
            if (!isOwnDevice)
            {
                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                response.IsSuccessful = false;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.DEVICE_MISMATCH,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.DEVICE_MISMATCH, language)}"
                };

                return response;
            }

            //response = await _authenticator.SendOtp(customer.AccountNumber, OtpTargetFeature.CompleteOnboarding);
            string message = await _otpService.CreateOtpMessage(customer.WalletNumber, language, OtpPurpose.QUESTIONS_RESET);
            await _notifier.SendSMS("9999999999", customer.WalletNumber, message);

            response.IsSuccessful = true;
            return response;
        }


        public async Task<BasicResponse> ResetQuestion(ResetQuestionsRequest request, string language)
        {
            var response = new BasicResponse(false);

            var customer = await _customerDAO.FindByWalletNumber(request.Username);

            if (customer == null)
            {
                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                response.IsSuccessful = false;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.CUSTOMER_NOT_FOUND,
                    ResponseDescription = $" {_messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language)}"
                };

                return response;
            }

            if (!customer.IsActive)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.UNAUTHORIZED,
                       ResponseCodes.PROFILE_DEACTIVATED, _messageProvider.GetMessage(ResponseCodes.PROFILE_DEACTIVATED, language));
            }

            var otp = await _otpDAO.Find(request.Username, OtpPurpose.QUESTIONS_RESET);
            if (otp == null)
            {
                return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.UNAUTHORIZED, ResponseCodes.CODE_VALIDATION_ERROR, _messageProvider.GetMessage(ResponseCodes.CODE_VALIDATION_ERROR, language));
            }

            if (_settings.IsTest==false)
            {
                if (DateTime.UtcNow > otp.DateCreated.AddMinutes(_settings.OtpDuration))
                {
                    return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.UNAUTHORIZED, ResponseCodes.CODE_VALIDATION_ERROR,
                                                               _messageProvider.GetMessage(ResponseCodes.CODE_VALIDATION_ERROR, language));

                }
            }


            if (!_cryptoService.AreEqual(request.Otp, otp.Code, otp.Salt))
            {
                return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.UNAUTHORIZED, ResponseCodes.CODE_VALIDATION_ERROR,
                    _messageProvider.GetMessage(ResponseCodes.CODE_VALIDATION_ERROR, language));
            }

            response = await _authenticator.ResetQuestions(request);
            if (!response.IsSuccessful)
            {

                if (response .Error?.ResponseCode == "ONB")
                {
                     response.Error.ResponseCode = ResponseCodes.INCORRECT_SECURITY_ANSWER; 
                    response.Error.ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INCORRECT_SECURITY_ANSWER , language);
                }
                else
                {
 response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                }
               
                //TODO: Determine fault type
            }
            return response;
        }


        public async Task<BasicResponse> InitiatePasswordReset(InitiatePasswordResetRequest request, string language)
        {
            var response = new BasicResponse(false);

            response = await _authenticator.ValidateAnswer(request.Username, request.Answer);
            if (!response.IsSuccessful)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.CLIENT_INVALID_ARGUMENT,
                      ResponseCodes.INCORRECT_SECURITY_ANSWER, _messageProvider.GetMessage(ResponseCodes.INCORRECT_SECURITY_ANSWER, language));
            }

            var message = await _otpService.CreateOtpMessage(request.Username, language, OtpPurpose.PASSWORD_RESET);
            _logger.LogInformation("RESET_PASSWORD_MESSAGE:{0}", message);
            await _notifier.SendSMS("9999999999", request.Username, message);

            response.IsSuccessful = true;

            return response;
        }


    }
}
