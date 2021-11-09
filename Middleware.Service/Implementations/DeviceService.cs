using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Core.DAO;
using Middleware.Core.Model;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.Model;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Newtonsoft.Json;

namespace Middleware.Service.Implementations
{
    public class DeviceService : IDeviceService
    {
        private readonly ICustomerDAO _customerDAO;
        private readonly IDeviceDAO _deviceDAO;
        private readonly IMessageProvider _messageProvider;
        private readonly IAuthenticator _authenticator;
        private readonly INotifier _notifier;
        private readonly ICryptoService _cryptoService;
        private readonly IOtpService _otpService;
        private readonly ISessionDAO _sessionDAO;
        private readonly IOtpDAO _otpDAO;
        private readonly SystemSettings _settings;
        private readonly ILogger _logger;
        private readonly IDeviceHistoryDAO _deviceHistoryDAO;
        //  private const int DEVICE_COUNT = 2;//TODO: Move to external source
        public DeviceService(ICustomerDAO customerDAO, IDeviceDAO deviceDAO, IMessageProvider messageProvider,
                                IAuthenticator authenticator, INotifier notifier, ICryptoService cryptoService, IOtpService otpService,
                                IOtpDAO otpDAO, IOptions<SystemSettings> settingsProvider, ISessionDAO sessionDAO, IDeviceHistoryDAO deviceHistoryDAO,
                                ILoggerFactory logger)
        {
            _customerDAO = customerDAO;
            _deviceDAO = deviceDAO;
            _messageProvider = messageProvider;
            _authenticator = authenticator;
            _notifier = notifier;
            _cryptoService = cryptoService;
            _otpService = otpService;
            _otpDAO = otpDAO;
            _settings = settingsProvider.Value;
            _logger = logger.CreateLogger(typeof(DeviceService));
            _sessionDAO = sessionDAO;
            _deviceHistoryDAO = deviceHistoryDAO;
        }

        public async Task<BasicResponse> CompleteDeviceActivation(DeviceActivationCompletionRequest request, string language)
        {
            _logger.LogInformation("Inside the CompleteDeviceActivation method of Dvice servie");
            var customer = await _customerDAO.FindByWalletNumber(request.UserName);
            if (customer == null)
            {
 
                _logger.LogInformation(" customer not found Inside the CompleteDeviceActivation method of Dvice servie Request==={request}",JsonConvert.SerializeObject(request));
 
                return ErrorResponse.Create<BasicResponse>(
                    FaultMode.REQUESTED_ENTITY_NOT_FOUND, ResponseCodes.CUSTOMER_NOT_FOUND,
                    _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language));
            }
            if (!customer.IsActive)
            {
                _logger.LogInformation(" customer not active  Inside the CompleteDeviceActivation method of Dvice servie Request==={request}", JsonConvert.SerializeObject(request));
                return ErrorResponse.Create<BasicResponse>(FaultMode.UNAUTHORIZED,
                       ResponseCodes.PROFILE_DEACTIVATED, _messageProvider.GetMessage(ResponseCodes.PROFILE_DEACTIVATED, language));
            }
            var currentDeviceCount = await _deviceDAO.CountAssignedDevices(customer.Id);
            if (currentDeviceCount >= _settings.MaxDeviceCount)
            {
                _logger.LogInformation(" DEVICE LIMIT REACHED  Inside the CompleteDeviceActivation method of Dvice servie Request==={request}", JsonConvert.SerializeObject(request));
                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.INVALID_OBJECT_STATE,
                    ResponseCodes.DEVICE_LIMIT_REACHED, _messageProvider.GetMessage(ResponseCodes.DEVICE_LIMIT_REACHED, language));
            }
            if (!(await _deviceDAO.IsAvailable(request.DeviceId)))
            {
                var findByCustomerId = await _deviceDAO.FindByCustomerId(customer.Id);
                bool isUsersAccountandDevice = findByCustomerId.Any(p => p.DeviceId == request.DeviceId);

                if (isUsersAccountandDevice == true)
                {
 
                    _logger.LogInformation("Device attached to the same  account =========DEVICE_NOT_AVAILABLE Inside the CompleteDeviceActivation method of Dvice servie Request==={request}", JsonConvert.SerializeObject(request));
                    return ErrorResponse.Create<BasicResponse>(FaultMode.INVALID_OBJECT_STATE,
                        ResponseCodes.DEVICE_ALREADY_ATACHED,
                        _messageProvider.GetMessage(ResponseCodes.DEVICE_ALREADY_ATACHED, language));
                }
                else
                {
                    _logger.LogInformation("Device attached to another account =========DEVICE_NOT_AVAILABLE Inside the CompleteDeviceActivation method of Dvice servie Request==={request}", JsonConvert.SerializeObject(request));
                    return ErrorResponse.Create<BasicResponse>(FaultMode.INVALID_OBJECT_STATE,
                        ResponseCodes.DEVICE_NOT_AVAILABLE,
                        _messageProvider.GetMessage(ResponseCodes.DEVICE_NOT_AVAILABLE, language));
                }

 
            }


            var otp = await _otpDAO.Find(request.UserName, OtpPurpose.DEVICE_SWITCH);
            if (otp == null)
            {
                _logger.LogInformation(" OTP is null Inside the CompleteDeviceActivation method of Dvice servie Request==={request}", JsonConvert.SerializeObject(new { request.UserName, OtpPurpose.DEVICE_SWITCH }));
                return ErrorResponse.Create<BasicResponse>(FaultMode.UNAUTHORIZED, ResponseCodes.CODE_VALIDATION_ERROR,
                                                           _messageProvider.GetMessage(ResponseCodes.CODE_VALIDATION_ERROR, language));
            }

            if (DateTime.UtcNow > otp.DateCreated.AddMinutes(_settings.OtpDuration))
            {
                _logger.LogInformation(" OTP is expired Inside the CompleteDeviceActivation method of Dvice servie Request==={request}", JsonConvert.SerializeObject(_settings.OtpDuration));
                return ErrorResponse.Create<BasicResponse>(FaultMode.UNAUTHORIZED, ResponseCodes.CODE_VALIDATION_ERROR,
                                                           _messageProvider.GetMessage(ResponseCodes.CODE_VALIDATION_ERROR, language));
            }



            if (!_cryptoService.AreEqual(request.Otp, otp.Code, otp.Salt))
            {
                _logger.LogInformation(" OTP mismatch Inside the CompleteDeviceActivation method of Dvice servie Request==={request}", JsonConvert.SerializeObject(new { request.Otp, otp.Code, otp.Salt }));
                return ErrorResponse.Create<BasicResponse>(FaultMode.UNAUTHORIZED, ResponseCodes.CODE_VALIDATION_ERROR,
                    _messageProvider.GetMessage(ResponseCodes.CODE_VALIDATION_ERROR, language));
            }
            var device = new Device
            {
                Customer = customer,
                Customer_Id = customer.Id,
                DateCreated = DateTime.UtcNow,
                DeviceId = request.DeviceId,
                IsActive = true,
                Model = request.DeviceModel
            };
            var transaction = _deviceDAO.Begin();
            _otpDAO.Join(transaction);

            try
            {
                await _deviceDAO.Add(device);
                await _otpDAO.Delete(otp.Id);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error completing device activation for user {0}", JsonConvert.SerializeObject(request));
                transaction.Rollback();
            }

            return new BasicResponse(true);
        }

        public async Task<ServiceResponse<DeviceStatus>> GetDeviceStatus(string deviceId, string language)
        {
            _logger.LogInformation("Inside the GetDeviceStatus  method of Dvice servie");
            var response = new ServiceResponse<DeviceStatus>(false);
            var device = await _deviceDAO.Find(deviceId);
            _logger.LogInformation("DEVICE: {0}", JsonConvert.SerializeObject(device));
            if (device == null)
            {
                return ErrorResponse.Create<ServiceResponse<DeviceStatus>>(
                  FaultMode.REQUESTED_ENTITY_NOT_FOUND, ResponseCodes.DEVICE_NOT_FOUND,
                  _messageProvider.GetMessage(ResponseCodes.DEVICE_NOT_FOUND, language));
            }

            response.IsSuccessful = true;
            response.SetPayload(new DeviceStatus
            {
                Assigned = device.Customer != null,
                DeviceId = device.DeviceId,
                DeviceModel = device.Model
            });

            return response;
        }

        public async Task<BasicResponse> InitiateDeviceActivation(DeviceActivationInitiationRequest request, string language)
        {
            _logger.LogInformation("Inside the InitiateDeviceActivation  method of Dvice servie");
            var customer = await _customerDAO.FindByWalletNumber(request.UserName);
            if (customer == null)
            {
                return ErrorResponse.Create<BasicResponse>(
                    FaultMode.REQUESTED_ENTITY_NOT_FOUND, ResponseCodes.CUSTOMER_NOT_FOUND,
                    _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language));
            }

            if (!customer.IsActive)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.UNAUTHORIZED,
                       ResponseCodes.PROFILE_DEACTIVATED, _messageProvider.GetMessage(ResponseCodes.PROFILE_DEACTIVATED, language));
            }

            var currentDeviceCount = await _deviceDAO.CountAssignedDevices(customer.Id);
            if (currentDeviceCount >= _settings.MaxDeviceCount)
            {
                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.INVALID_OBJECT_STATE,
                    ResponseCodes.DEVICE_LIMIT_REACHED, _messageProvider.GetMessage(ResponseCodes.DEVICE_LIMIT_REACHED, language));
            }
            if (!(await _deviceDAO.IsAvailable(request.DeviceId)))
            {
                //_logger.LogInformation("Device attached to another account =========DEVICE_NOT_AVAILABLE Inside the InitiateDeviceActivation method of Dvice servie Request==={request}", JsonConvert.SerializeObject(request));

                //return ErrorResponse.Create<BasicResponse>(
                //    FaultMode.INVALID_OBJECT_STATE, ResponseCodes.DEVICE_NOT_AVAILABLE,
                //    _messageProvider.GetMessage(ResponseCodes.DEVICE_NOT_AVAILABLE, language));

                var findByCustomerId = await _deviceDAO.FindByCustomerId(customer.Id);
                bool isUsersAccountandDevice = findByCustomerId.Any(p => p.DeviceId == request?.DeviceId);

                if (isUsersAccountandDevice == true)
                {
                    _logger.LogInformation("Device attached to another account =========DEVICE_NOT_AVAILABLE Inside the InitiateDeviceActivation method of Dvice servie Request==={request}", JsonConvert.SerializeObject(request));
                    return ErrorResponse.Create<BasicResponse>(FaultMode.INVALID_OBJECT_STATE,
                        ResponseCodes.DEVICE_ALREADY_ATACHED,
                        _messageProvider.GetMessage(ResponseCodes.DEVICE_ALREADY_ATACHED, language));
                }
                else
                {
                    _logger.LogInformation("Device attached to another account =========DEVICE_NOT_AVAILABLE Inside the InitiateDeviceActivation method of Dvice servie Request==={request}", JsonConvert.SerializeObject(request));
                    return ErrorResponse.Create<BasicResponse>(FaultMode.INVALID_OBJECT_STATE,
                        ResponseCodes.DEVICE_NOT_AVAILABLE,
                        _messageProvider.GetMessage(ResponseCodes.DEVICE_NOT_AVAILABLE, language));
                }
            }

 

            var response = await _authenticator.ValidateAnswer(request.UserName, request.Answer);
            if (!response.IsSuccessful)
            {
                return response;
            }
            string message = await _otpService.CreateOtpMessage(request.UserName, language, OtpPurpose.DEVICE_SWITCH);

            await _notifier.SendSMS("9999999999", customer.WalletNumber, message);
            response.IsSuccessful = true;
            return response;
        }

    }
}
