using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Core.DAO;
using Middleware.Core.DTO;
//using Middleware.Core.Model;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.Model;
using Middleware.Service.Onboarding;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Newtonsoft.Json;

namespace Middleware.Service.Implementations
{
    #region Old
    //public class AuthenticationService : IAuthenticationServices
    //{
    //    private readonly IWalletOpeningRequestDAO _requestDAO;
    //    readonly IAuthenticator _authenticator;
    //    readonly ISessionDAO _sessionDAO;
    //    readonly ICustomerDAO _customerDAO;
    //    readonly IDocumentDAO _documentDAO;
    //    readonly IMessageProvider _messageProvider;
    //    readonly IDeviceDAO _deviceDAO;
    //    readonly ILogger _logger;
    //    private readonly IImageManager _imageManager;
    //    readonly SystemSettings _settings;
    //    //  private const int DEVICE_COUNT = 2;//TODO: Move to external source
    //    private readonly int DEVICE_COUNT = 0;//TODO: Move to external source
    //    public AuthenticationService(IAuthenticator authenticator, ICustomerDAO customerDAO, IDocumentDAO documentDAO,
    //                           ISessionDAO sessionDAO, IMessageProvider messageProvider, IDeviceDAO deviceDAO,
    //                           ILoggerFactory logger, IOptions<SystemSettings> settingsProvider, IWalletOpeningRequestDAO requestDAO, IImageManager imageManager)
    //    {
    //        _authenticator = authenticator;
    //        _sessionDAO = sessionDAO;
    //        _messageProvider = messageProvider;
    //        _deviceDAO = deviceDAO;
    //        _customerDAO = customerDAO;
    //        _documentDAO = documentDAO;
    //        _logger = logger.CreateLogger(typeof(AuthenticationService));
    //        _settings = settingsProvider.Value;
    //        _requestDAO = requestDAO;
    //        _imageManager = imageManager;
    //        DEVICE_COUNT = _settings.MaxDeviceCount;
    //    }

    //    private async Task<ServiceResponse<LoginResponse>> AddDeviceIfAllowed(long customerId, string language)
    //    {
    //        var deviceCount = await _deviceDAO.CountAssignedDevices(customerId);
    //        if (deviceCount <= DEVICE_COUNT)
    //        {
    //            return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.INVALID_OBJECT_STATE,
    //                ResponseCodes.NEW_DEVICE_DETECTED, _messageProvider.GetMessage(ResponseCodes.NEW_DEVICE_DETECTED, language));
    //        }
    //        else
    //        {
    //            return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.LIMIT_EXCEEDED,
    //                ResponseCodes.DEVICE_LIMIT_REACHED, _messageProvider.GetMessage(ResponseCodes.DEVICE_LIMIT_REACHED, language));
    //        }


    //    }

    //    public async Task<ServiceResponse<LoginResponse>> Authenticate(LoginRequest request, string language)
    //    {

    //        _logger.LogInformation("Inside the Authenticate method of AuthenticationService ");

    //        //_logger.LogInformation($"CREDENTIAL: {JsonConvert.SerializeObject(request)}");

    //        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
    //        {
    //            _logger.LogInformation("Invalid credeentials {0}", JsonConvert.SerializeObject(request));
    //            return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.CLIENT_INVALID_ARGUMENT,
    //             ResponseCodes.INVALID_LOGIN_DETAILS, $"{_messageProvider.GetMessage(ResponseCodes.INVALID_LOGIN_DETAILS, language)}");

    //        }

    //        request.UserName = request.UserName.Trim().ToLower();
    //        //This line of code used to disable the supposed action: you can activate it by removing the comment below;
    //        var hasReachedMaximumLoginFailedCount = false;//  await _customerDAO.HasReachedLoginFailCount(request.UserName);

    //        if (hasReachedMaximumLoginFailedCount)
    //        {

    //            return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.RESET_PASSWORD,
    //             ResponseCodes.RESET_PASSWORD, $"{_messageProvider.GetMessage(ResponseCodes.RESET_PASSWORD, language)}");

    //        }
    //        if (request.IsValid(out var source) == false)
    //        {

    //            return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.CLIENT_INVALID_ARGUMENT,
    //                ResponseCodes.INVALID_INPUT_PARAMETER, $"{_messageProvider.GetMessage(ResponseCodes.INVALID_LOGIN_DETAILS, language)} - {source}");
    //        }
    //        var customer = await _customerDAO.FindByWalletNumber(request.UserName);
    //        if (customer == null)
    //        {


    //            if (hasReachedMaximumLoginFailedCount == true)
    //            {

    //                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.RESET_PASSWORD,
    //                 ResponseCodes.RESET_PASSWORD, $"{_messageProvider.GetMessage(ResponseCodes.RESET_PASSWORD, language)} - {source}");

    //            }
    //            else
    //            {
    //                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(
    //                                  FaultMode.UNAUTHORIZED, ResponseCodes.INVALID_LOGIN_DETAILS,
    //                                  _messageProvider.GetMessage(ResponseCodes.INVALID_LOGIN_DETAILS, language));

    //            }
    //        }
    //        if (customer.IsActive == false)
    //        {
    //            _logger.LogInformation("The customer profile has been deactivated ");
    //            return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.DEACTIVATED,
    //                                   ResponseCodes.PROFILE_DEACTIVATED, _messageProvider.GetMessage(ResponseCodes.PROFILE_DEACTIVATED, language));


    //        }
    //        _logger.LogInformation("Payload to t _authenticator.AuthenticateWithPhone Inside the Authenticate method of AuthenticationService {payload}", JsonConvert.SerializeObject(new { username = request.UserName, password = request.Password }));
    //        var authResponse = await _authenticator.AuthenticateWithPhone(request.UserName, request.Password);

    //        if (authResponse.IsSuccessful == false)
    //        {
    //            _logger.LogInformation("The customer is not authorized");

    //            return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.AUTHTENTIATION_FAILURE,
    //                             authResponse.Error.ResponseCode, _messageProvider.GetMessage(ResponseCodes.INVALID_LOGIN_DETAILS, language));


    //        }

    //        _logger.LogInformation("Calling the   _deviceDAO.Find in the Authenticate method of the AuthemticationService");
    //        var device = await _deviceDAO.Find(request.DeviceId);
    //        _logger.LogInformation($"LOG IN DEVICE CHECK: ", JsonConvert.SerializeObject(device));
    //        if (device != null)
    //        {
    //            if (device.Customer == null)
    //            {
    //                _logger.LogInformation("Calling the   AddDeviceIfAllowed in the Authenticate method of the AuthemticationService . payload ====>{0}", JsonConvert.SerializeObject(new { customer.Id, language }));
    //                var data = await AddDeviceIfAllowed(customer.Id, language);
    //                _logger.LogInformation("Calling the   AddDeviceIfAllowed in the Authenticate method of the AuthemticationService . response ====>{0}", JsonConvert.SerializeObject(data));

    //                return data;
    //            }

    //            // if (device.IsActive==true&&(device.Customer_Id!= customer.Id))
    //            var deviceCount = await _deviceDAO.CountAssignedDevices(customer.Id);
    //            if (device.IsActive == false)
    //            {
    //                _logger.LogInformation("The device isactive  is false:  is disabled");
    //                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.UNAUTHORIZED,
    //                    ResponseCodes.DEVICE_DISABLED, _messageProvider.GetMessage(ResponseCodes.DEVICE_DISABLED, language));
    //            }
    //            else if (device.IsActive == false && device.Customer_Id == null)
    //            {
    //                _logger.LogInformation("The device isactive  is false and customer id  is null:  is disabled");
    //                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.UNAUTHORIZED,
    //                    ResponseCodes.DEVICE_DISABLED, _messageProvider.GetMessage(ResponseCodes.DEVICE_DISABLED, language));
    //            }
    //            else if (device.IsActive == true && device.Customer_Id == null && customer != null)
    //            {
    //                if (deviceCount < DEVICE_COUNT)
    //                {
    //                    _logger.LogInformation("The device isactive is true but the device is under released condition ");
    //                    return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.NONE,
    //                             ResponseCodes.USE_THIS_DEVICE, _messageProvider.GetMessage(ResponseCodes.USE_THIS_DEVICE, language));
    //                }



    //            }

    //            else if (device.IsActive == true && device.Customer_Id != null && device.Customer_Id != customer.Id)
    //            {
    //                _logger.LogInformation("The device isactive is true but the customerid and the device.customerid are not the same");
    //                _logger.LogInformation("CustomerId= {csId}.   Device.CustomerId={dcsId}", customer.Id, device.Customer_Id);

    //                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.UNAUTHORIZED,
    //                   ResponseCodes.DEVICE_MISMATCH, _messageProvider.GetMessage(ResponseCodes.DEVICE_MISMATCH, language));
    //            }

    //        }
    //        else
    //        {

    //            return await AddDeviceIfAllowed(customer.Id, language);

    //        }

    //        var hasUploaded = await _documentDAO.HasDocuments(customer.Id, Core.DTO.DocumentType.IDENTIFICATION); //should this really be here?????
    //        var photoUrl = await _requestDAO.Find(request.UserName);
    //        // string directoryPath = _imageManager.GetFileViewUrl(photoUrl.PhotoLocation, _settings.ReturnedBaseUrl);
    //        string directoryPath = _imageManager.GetImage(photoUrl.PhotoLocation, _settings.PhotoReturnedBaseUrl);

    //        var document = await _documentDAO.FindByPhoneNumber(request.UserName);
    //        bool approved = KYCDocumentApproved(document);

    //        _logger.LogInformation("Photo Returned Base Url {0}", directoryPath);


    //        var response = new ServiceResponse<LoginResponse>(true);
    //        var result = new LoginResponse
    //        {
    //            FirstName = customer.FirstName,
    //            LastName = customer.LastName,
    //            LastLogin = customer.LastLogin,
    //            AuthenticationToken = Guid.NewGuid().ToString(),
    //            LocalBankCode = _settings.BankCode,
    //            LocalWalletCode = _settings.WalletCode,
    //            // PhotoUrl = directoryPath.Replace("\\", "//"),
    //            PhotoUrl = directoryPath,
    //            HasUploadedKYC = hasUploaded,
    //            ReferralCode = customer.ReferralCode,
    //            IsApproved = approved,
    //            Id = customer.Id


    //        };
    //        _logger.LogInformation("Respponse to front end {0}", JsonConvert.SerializeObject(result));
    //        response.SetPayload(result);
    //        var sessionTicket = _sessionDAO.Begin();
    //        _customerDAO.Join(sessionTicket);

    //        try
    //        {
    //            await _sessionDAO.DeleteCustomerSessions(customer.Id);
    //            customer.LastLogin = DateTime.Now;
    //            var data = authResponse.GetPayload();
    //            if (data.HasBank && !customer.HasAccount)
    //            {
    //                customer.BankId = data.BankId;
    //                customer.HasAccount = true;
    //            }
    //            var session = new Session
    //            {
    //                Customer = customer,
    //                StartDate = DateTime.UtcNow,
    //                Token = result.AuthenticationToken,
    //                LastActiveTime = DateTime.Now,
    //                BankId = customer.BankId,
    //                Customer_Id = customer.Id,
    //                WalletNumber = customer.WalletNumber,
    //                UserName = customer.WalletNumber
    //            };
    //            await _sessionDAO.Add(session);

    //            await _customerDAO.Update(customer);
    //            sessionTicket.Commit();


    //            return response;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogCritical(ex, "Error on Login for {0}", request.UserName);
    //            sessionTicket.Rollback();
    //            response.IsSuccessful = false;
    //            if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
    //            {
    //                response.Error = new ErrorResponse()
    //                {
    //                    ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
    //                    ResponseDescription =
    //                        _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
    //                };
    //                return response;
    //            }
    //            return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.EXISTS,
    //                ResponseCodes.GENERAL_ERROR, _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language));
    //        }
    //    }

    //    private static bool KYCDocumentApproved(IEnumerable<Core.Model.Document> document)
    //    {
    //        if (document != null && document.Count() > 0)
    //        {
    //            var kycDocumentApprovedAcount = document.Count(p => p.Status == DocumentStatus.APPROVED);
    //            var kycDocumentCount = document.Count();

    //            bool approved = kycDocumentApprovedAcount == kycDocumentCount;
    //            return approved;
    //        }
    //        return false;
    //    }

    //    public async Task EndSession(string authToken)
    //    {
    //        await _sessionDAO.Delete(authToken);
    //        return;
    //    }

    //    public async Task<ServiceResponse<AuthenticatedUser>> ValidateSession(string authToken)
    //    {
    //        var response = new ServiceResponse<AuthenticatedUser>(false);
    //        var session = await _sessionDAO.Find(authToken);
    //        if (session == null || session.EndDate != null)
    //        {
    //            response.Error = new ErrorResponse
    //            {
    //                ResponseCode = ResponseCodes.INVALID_SESSION,
    //                ResponseDescription = "Invalid Session 1"
    //            };
    //            return response;
    //        }

    //        response.IsSuccessful = true;
    //        response.SetPayload(new AuthenticatedUser
    //        {
    //            BankId = session.BankId,
    //            WalletNumber = session.WalletNumber,
    //            Id = session.Customer_Id,
    //            UserName = session.UserName
    //        });
    //        return response;
    //    }

    //    public async Task<ServiceResponse<SecurityQuestion>> GetSecurityQuestion(string username, string language)
    //    {
    //        var response = new ServiceResponse<SecurityQuestion>(false);
    //        try
    //        {
    //            _logger.LogInformation("Inside the GetSecurityQuestion method of the AuthenticationService at {0}", DateTime.UtcNow);


    //            // var usernameIsEmail = Util.IsEmail(username);
    //            var customer = await _customerDAO.FindByWalletNumber(username);

    //            if (customer == null)
    //            {
    //                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
    //                response.IsSuccessful = false;
    //                response.Error = new ErrorResponse
    //                {
    //                    ResponseCode = ResponseCodes.CUSTOMER_NOT_FOUND,
    //                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language)}"
    //                };

    //                return response;
    //            }

    //            if (!customer.IsActive)
    //            {
    //                return ErrorResponse.Create<ServiceResponse<SecurityQuestion>>(FaultMode.UNAUTHORIZED,
    //                       ResponseCodes.PROFILE_DEACTIVATED, _messageProvider.GetMessage(ResponseCodes.PROFILE_DEACTIVATED, language));
    //            }
    //            response = await _authenticator.GetSecurityQuestion(customer.WalletNumber);

    //            if (!response.IsSuccessful)
    //            {
    //                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
    //            }

    //            return response;
    //        }
    //        catch (Exception ex)
    //        {
    //            if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
    //            {
    //                response.Error = new ErrorResponse()
    //                {
    //                    ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
    //                    ResponseDescription =
    //                        _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
    //                };
    //                return response;
    //            }
    //            _logger.LogCritical(ex, "Server error occurred in the GetSecurityQuestion method of the AuthenticationService at {0} ", DateTime.UtcNow);
    //            return null;
    //        }
    //    }


    //}


    #endregion



    public class AuthenticationService : IAuthenticationServices
    {
        private readonly IWalletOpeningRequestDAO _requestDAO;
        readonly IAuthenticator _authenticator;
        readonly ISessionDAO _sessionDAO;
        readonly ICustomerDAO _customerDAO;
        readonly IDocumentDAO _documentDAO;
 
        readonly ICaseDAO  _caseDAO;
 
        readonly IMessageProvider _messageProvider;
        readonly IDeviceDAO _deviceDAO;
        readonly ILogger _logger;
        private readonly IImageManager _imageManager;
        readonly SystemSettings _settings;
        //  private const int DEVICE_COUNT = 2;//TODO: Move to external source
        private readonly int DEVICE_COUNT = 0;//TODO: Move to external source
        public AuthenticationService(IAuthenticator authenticator, ICustomerDAO customerDAO, IDocumentDAO documentDAO,
                               ISessionDAO sessionDAO, IMessageProvider messageProvider, IDeviceDAO deviceDAO,
                               ILoggerFactory logger, IOptions<SystemSettings> settingsProvider, IWalletOpeningRequestDAO requestDAO, IImageManager imageManager, ICaseDAO caseDAO)
        {
            _authenticator = authenticator;
            _sessionDAO = sessionDAO;
            _messageProvider = messageProvider;
            _deviceDAO = deviceDAO;
            _customerDAO = customerDAO;
            _documentDAO = documentDAO;
            _logger = logger.CreateLogger(typeof(AuthenticationService));
            _settings = settingsProvider.Value;
            _requestDAO = requestDAO;
            _imageManager = imageManager;
            DEVICE_COUNT = _settings.MaxDeviceCount;
            _caseDAO = caseDAO;
        }

        private async Task<ServiceResponse<LoginResponse>> AddDeviceIfAllowed(long customerId, string language)
        {
            var deviceCount = await _deviceDAO.CountAssignedDevices(customerId);
            if (deviceCount <= DEVICE_COUNT)
            {
                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.INVALID_OBJECT_STATE,
                    ResponseCodes.NEW_DEVICE_DETECTED, _messageProvider.GetMessage(ResponseCodes.NEW_DEVICE_DETECTED, language));
            }
            else
            {
                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.LIMIT_EXCEEDED,
                    ResponseCodes.DEVICE_LIMIT_REACHED, _messageProvider.GetMessage(ResponseCodes.DEVICE_LIMIT_REACHED, language));
            }


        }

        public async Task<ServiceResponse<LoginResponse>> Authenticate(LoginRequest request, string language)
        {
            _logger.LogInformation("Inside the Authenticate method of AuthenticationService ");
            //_logger.LogInformation($"CREDENTIAL: {JsonConvert.SerializeObject(request)}");

            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogInformation("Invalid credeentials {0}", JsonConvert.SerializeObject(request));
                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.CLIENT_INVALID_ARGUMENT,
                 ResponseCodes.INVALID_LOGIN_DETAILS, $"{_messageProvider.GetMessage(ResponseCodes.INVALID_LOGIN_DETAILS, language)}");

            }

            request.UserName = request.UserName.Trim().ToLower();
            //This line of code used to disable the supposed action: you can activate it by removing the comment below;
            var hasReachedMaximumLoginFailedCount = false;//  await _customerDAO.HasReachedLoginFailCount(request.UserName);

            if (hasReachedMaximumLoginFailedCount)
            {

                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.RESET_PASSWORD,
                 ResponseCodes.RESET_PASSWORD, $"{_messageProvider.GetMessage(ResponseCodes.RESET_PASSWORD, language)}");

            }
            if (request.IsValid(out var source) == false)
            {

                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.CLIENT_INVALID_ARGUMENT,
                    ResponseCodes.INVALID_INPUT_PARAMETER, $"{_messageProvider.GetMessage(ResponseCodes.INVALID_LOGIN_DETAILS, language)} - {source}");
            }
            var customer = await _customerDAO.FindByWalletNumber(request.UserName);
            if (customer == null)
            {


                if (hasReachedMaximumLoginFailedCount == true)
                {

                    return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.RESET_PASSWORD,
                     ResponseCodes.RESET_PASSWORD, $"{_messageProvider.GetMessage(ResponseCodes.RESET_PASSWORD, language)} - {source}");

                }
                else
                {
                    return ErrorResponse.Create<ServiceResponse<LoginResponse>>(
                                      FaultMode.UNAUTHORIZED, ResponseCodes.INVALID_LOGIN_DETAILS,
                                      _messageProvider.GetMessage(ResponseCodes.INVALID_LOGIN_DETAILS, language));

                }
            }
            if (customer.IsActive == false)
            {
                _logger.LogInformation("The customer profile has been deactivated ");
                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.DEACTIVATED,
                                       ResponseCodes.PROFILE_DEACTIVATED, _messageProvider.GetMessage(ResponseCodes.PROFILE_DEACTIVATED, language));


            }
            var authResponse = await _authenticator.AuthenticateWithPhone(request.UserName, request.Password);

            if (authResponse.IsSuccessful == false)
            {
                _logger.LogInformation("The customer is not authorized");

                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.AUTHTENTIATION_FAILURE,
                                 authResponse.Error.ResponseCode, _messageProvider.GetMessage(ResponseCodes.INVALID_LOGIN_DETAILS, language));


            }

            _logger.LogInformation("Calling the   _deviceDAO.Find in the Authenticate method of the AuthemticationService");
            var device = await _deviceDAO.Find(request.DeviceId);
            _logger.LogInformation($"LOG IN DEVICE CHECK: ", JsonConvert.SerializeObject(device));
            if (device != null)
            {
                if (device.Customer == null)
                {
                    _logger.LogInformation("Calling the   AddDeviceIfAllowed in the Authenticate method of the AuthemticationService . payload ====>{0}", JsonConvert.SerializeObject(new { customer.Id, language }));
                    var data = await AddDeviceIfAllowed(customer.Id, language);
                    _logger.LogInformation("Calling the   AddDeviceIfAllowed in the Authenticate method of the AuthemticationService . response ====>{0}", JsonConvert.SerializeObject(data));

                    return data;
                }

                // if (device.IsActive==true&&(device.Customer_Id!= customer.Id))
                var deviceCount = await _deviceDAO.CountAssignedDevices(customer.Id);
                if (device.IsActive == false)
                {
                    _logger.LogInformation("The device isactive  is false:  is disabled");
                    return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.UNAUTHORIZED,
                        ResponseCodes.DEVICE_DISABLED, _messageProvider.GetMessage(ResponseCodes.DEVICE_DISABLED, language));
                }
                else if (device.IsActive == false && device.Customer_Id == null)
                {
                    _logger.LogInformation("The device isactive  is false and customer id  is null:  is disabled");
                    return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.UNAUTHORIZED,
                        ResponseCodes.DEVICE_DISABLED, _messageProvider.GetMessage(ResponseCodes.DEVICE_DISABLED, language));
                }
                else if (device.IsActive == true && device.Customer_Id == null && customer != null)
                {
                    if (deviceCount < DEVICE_COUNT)
                    {
                        _logger.LogInformation("The device isactive is true but the device is under released condition ");
                        return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.NONE,
                                 ResponseCodes.USE_THIS_DEVICE, _messageProvider.GetMessage(ResponseCodes.USE_THIS_DEVICE, language));
                    }



                }

                else if (device.IsActive == true && device.Customer_Id != null && device.Customer_Id != customer.Id)
                {
                    _logger.LogInformation("The device isactive is true but the customerid and the device.customerid are not the same");
                    _logger.LogInformation("CustomerId= {csId}.   Device.CustomerId={dcsId}", customer.Id, device.Customer_Id);

                    return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.UNAUTHORIZED,
                       ResponseCodes.DEVICE_MISMATCH, _messageProvider.GetMessage(ResponseCodes.DEVICE_MISMATCH, language));
                }

            }
            else
            {

                return await AddDeviceIfAllowed(customer.Id, language);

            }

            var hasUploaded = await _documentDAO.HasDocuments(customer.Id, Core.DTO.DocumentType.IDENTIFICATION); //should this really be here?????
            var photoUrl = await _requestDAO.Find(request.UserName);
       
            string directoryPath = _imageManager.GetImage(photoUrl.PhotoLocation, _settings.PhotoReturnedBaseUrl);
 
 
 
            var cases = await _caseDAO.Find(customer.Id, AccountType.WALLET); 

            bool approved = KYCDocumentApproved(cases?.Documents);

            _logger.LogInformation("Photo Returned Base Url {0}", directoryPath);


            var response = new ServiceResponse<LoginResponse>(true);
            var result = new LoginResponse
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                LastLogin = customer.LastLogin,
                AuthenticationToken = Guid.NewGuid().ToString(),
                LocalBankCode = _settings.BankCode,
                LocalWalletCode = _settings.WalletCode,
                // PhotoUrl = directoryPath.Replace("\\", "//"),
                PhotoUrl = directoryPath,
                HasUploadedKYC = hasUploaded,
                ReferralCode = customer.ReferralCode,
                IsApproved = approved,
                Id = customer.Id


            };
            _logger.LogInformation("Respponse to front end {0}", JsonConvert.SerializeObject(result));
            response.SetPayload(result);
            var sessionTicket = _sessionDAO.Begin();
            _customerDAO.Join(sessionTicket);

            try
            {
                await _sessionDAO.DeleteCustomerSessions(customer.Id);
                customer.LastLogin = DateTime.Now;
                var data = authResponse.GetPayload();
                if (data.HasBank && !customer.HasAccount)
                {
                    customer.BankId = data.BankId;
                    customer.HasAccount = true;
                }
                var session = new Session
                {
                    Customer = customer,
                    StartDate = DateTime.UtcNow,
                    Token = result.AuthenticationToken,
                    LastActiveTime = DateTime.Now,
                    BankId = customer.BankId,
                    Customer_Id = customer.Id,
                    WalletNumber = customer.WalletNumber,
                    UserName = customer.WalletNumber
                };
                await _sessionDAO.Add(session);

                await _customerDAO.Update(customer);
                sessionTicket.Commit();


                return response;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error on Login for {0}", request.UserName);
                sessionTicket.Rollback();
                response.IsSuccessful = false;
                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
                {
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                        ResponseDescription =
                            _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                    };
                    return response;
                }
                return ErrorResponse.Create<ServiceResponse<LoginResponse>>(FaultMode.EXISTS,
                    ResponseCodes.GENERAL_ERROR, _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language));
            }
        }

        //private static bool KYCDocumentApprovedOld(IEnumerable<Document> document)
        //{
        //    var kycDocumentApprovedAcount = document.Count(p => p.Status == DocumentStatus.APPROVED);
        //    var kycDocumentCount = document.Count();

        //    bool approved = kycDocumentApprovedAcount == kycDocumentCount;
        //    return approved;
        //}
 
        private bool KYCDocumentApproved(IEnumerable<Core.Model.Document> document)
        {
            _logger.LogInformation("Inside the KYCDocumentApproved method of the Autentication service");
            if (document != null && document.Count() > 0)
 
            {
                var kycDocumentApprovedAcount = document.Count(p => p.Status == DocumentStatus.APPROVED || p.State== DocumentState.VALID);
                var kycDocumentApprovedAcountReject = document.Count(p =>   p.State== DocumentState.INVALID);
                var kycDocumentCount = document.Count()- kycDocumentApprovedAcountReject;

                bool approved = kycDocumentApprovedAcount == kycDocumentCount;
                return approved;
            }
            return false;
        }

        public async Task EndSession(string authToken)
        {
            await _sessionDAO.Delete(authToken);
            return;
        }

        public async Task<ServiceResponse<AuthenticatedUser>> ValidateSession(string authToken)
        {
            var response = new ServiceResponse<AuthenticatedUser>(false);
            var session = await _sessionDAO.Find(authToken);
            if (session == null || session.EndDate != null)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INVALID_SESSION,
                    ResponseDescription = "Invalid Session 1"
                };
                return response;
            }

            response.IsSuccessful = true;
            response.SetPayload(new AuthenticatedUser
            {
                BankId = session.BankId,
                WalletNumber = session.WalletNumber,
                Id = session.Customer_Id,
                UserName = session.UserName
            });
            return response;
        }

        public async Task<ServiceResponse<SecurityQuestion>> GetSecurityQuestion(string username, string language)
        {
            var response = new ServiceResponse<SecurityQuestion>(false);
            try
            {
                _logger.LogInformation("Inside the GetSecurityQuestion method of the AuthenticationService at {0}", DateTime.UtcNow);


                // var usernameIsEmail = Util.IsEmail(username);
                var customer = await _customerDAO.FindByWalletNumber(username);

                if (customer == null)
                {
                    response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                    response.IsSuccessful = false;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.CUSTOMER_NOT_FOUND,
                        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language)}"
                    };

                    return response;
                }

                if (!customer.IsActive)
                {
                    return ErrorResponse.Create<ServiceResponse<SecurityQuestion>>(FaultMode.UNAUTHORIZED,
                           ResponseCodes.PROFILE_DEACTIVATED, _messageProvider.GetMessage(ResponseCodes.PROFILE_DEACTIVATED, language));
                }
                response = await _authenticator.GetSecurityQuestion(customer.WalletNumber);

                if (!response.IsSuccessful)
                {
                    response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                }

                return response;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
                {
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                        ResponseDescription =
                            _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                    };
                    return response;
                }
                _logger.LogCritical(ex, "Server error occurred in the GetSecurityQuestion method of the AuthenticationService at {0} ", DateTime.UtcNow);
                return null;
            }
        }


    }
}
