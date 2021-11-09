using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Middleware.Service.DAO;
using Middleware.Service.Model;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Middleware.Core.Model;
using Middleware.Core.DAO;
using Microsoft.Extensions.Options;
using Middleware.Core.DTO;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Document = Middleware.Core.Model.Document;

namespace Middleware.Service.Implementations
{
    public class WalletOpeningService : IWalletOpeningService
    {
        private readonly ILogger _logger;
        private readonly IMessageProvider _messageProvider;
        private readonly IProfileManager _profileManager;
        private readonly IWalletOpeningRequestDAO _requestDAO;
        private readonly ICustomerDAO _customerDAO;
        private readonly INotifier _notifier;
        private readonly IWalletCreator _walletCreator;
        private readonly IImageManager _imageManager;
        private readonly IDeviceDAO _deviceDAO;
        private readonly ICryptoService _cryptoService;
        private readonly IOtpService _otpService;
        private readonly ICaseDAO _caseDAO;
        private readonly IDocumentDAO _documentDAO;
        private readonly IOtpDAO _otpDAO;
        private readonly IWalletService _wallet;
        private readonly ICodeGenerator _codeGenerator;
        private readonly SystemSettings _settings;
        private readonly IConfiguration _config;
        public WalletOpeningService(IMessageProvider messageProvider, ILoggerFactory logger,
            IProfileManager profileManager, IWalletOpeningRequestDAO requestDAO,
            ICustomerDAO customerDAO, INotifier notifier, IWalletCreator walletCreator,
            IImageManager imageManager, IDeviceDAO deviceDAO,
            ICryptoService cryptoService, IOtpService otpService, ICaseDAO caseDAO, IDocumentDAO documentDAO,
            IOtpDAO otpDAO, IOptions<SystemSettings> settingsProvider, IWalletService wallet, ICodeGenerator codeGenerator, IConfiguration conf)
        {
            _messageProvider = messageProvider;
            _logger = logger.CreateLogger(typeof(WalletOpeningService));
            _profileManager = profileManager;
            _requestDAO = requestDAO;
            _customerDAO = customerDAO;
            _notifier = notifier;
            _walletCreator = walletCreator;
            _imageManager = imageManager;
            _deviceDAO = deviceDAO;
            _otpService = otpService;
            _caseDAO = caseDAO;
            _documentDAO = documentDAO;
            _cryptoService = cryptoService;
            _otpDAO = otpDAO;
            _wallet = wallet;
            _settings = settingsProvider.Value;
            _codeGenerator = codeGenerator;
            _config = conf;
        }

        public object SenegalCode()
        {
            var data = new { code = _settings.BankCode, wallet = _settings.WalletCode };
            return data;
        }

        
        public async Task<BasicResponse> AddIdentificationDocument(IdUpdateRequest request, string language)
        {

            try
            {
                var response = new BasicResponse(false);
                _logger.LogInformation("Inside the AddIdentificationDocument of the WalletOpeningService at {0}", DateTime.UtcNow);

                if (request.IsValid(out string source) == false)
                {

                    response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}"
                    };
                    return response;

                }


                var savedRequest = await _requestDAO.Find(request.PhoneNumber);
                if (savedRequest == null)
                {
                    response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.REQUEST_NOT_FOUND,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language)
                    };
                    return response;
                }
                if (savedRequest.Status == WalletOpeningStatus.COMPLETED)
                {
                    _logger.LogInformation("{0}==========================> HAS ALREADY ONBORDED", request.PhoneNumber);
                    response.FaultType = FaultMode.INVALID_OBJECT_STATE;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.WALLET_ALREADY_OPENED,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_WALLET_REGISTRATION_STATE, language)
                    };
                    return response;
                }
                savedRequest.Nationality = request.Nationality;
                savedRequest.IdType = request.IdType;
                savedRequest.IdNumber = request.IdNumber;
                savedRequest.Status = WalletOpeningStatus.ID_PROVIDED;
                _logger.LogInformation("Calling  RequestDAO.Update Inside the AddIdentificationDocument of WalletOpening Service . Request:=====>{0}", JsonConvert.SerializeObject(savedRequest));
                await _requestDAO.Update(savedRequest);
                response.IsSuccessful = true;
                return response;
            }
            catch (Exception ex)
            {


                _logger.LogCritical(ex, "Error occurred in the AddIdentificationDocument of the WalletOpeningService at {0}", DateTime.UtcNow);

                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("network-related or instance-specific error"))
                {
                    return new BasicResponse { Error = new ErrorResponse { ResponseCode = ResponseCodes.SQL_DATABASE_NETWORK_ERROR, ResponseDescription = _messageProvider.GetMessage(ResponseCodes.SQL_DATABASE_NETWORK_ERROR, language) } };

                }
                return new BasicResponse { Error = new ErrorResponse { ResponseCode = ResponseCodes.GENERAL_ERROR, ResponseDescription = ex.Message } };
            }
        }

        public async Task<ServiceResponse<PhotoUploadResponse>> AddPhoto(PhotoUpdateRequest request, string language)
        {
            var response = new ServiceResponse<PhotoUploadResponse>(false);
            var imagePath = string.Empty;
            try
            {
                _logger.LogInformation("Inside the AddPhoto  of the WalletOpeningService at {0}", DateTime.UtcNow);


                if (string.IsNullOrWhiteSpace(language))
                {

                    _logger.LogInformation(" Language is null Inside the AddPhoto  of the WalletOpeningService at {0}", DateTime.UtcNow);


                    response.Error = new ErrorResponse { ResponseCode = ResponseCodes.REQUEST_MISMATCH, ResponseDescription = "Language can not be null" };
                    response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                    response.IsSuccessful = false;
                }
                if (request.IsValid(out string sourrce) == false)
                {
                    response.Error = new ErrorResponse { ResponseCode = ResponseCodes.REQUEST_MISMATCH, ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.REQUEST_MISMATCH, language)} - {sourrce}" };
                }




                //request.Picture = "Picture byteArray removed for brevity";
                _logger.LogInformation("PhotoUpdateRequest=>{0}", JsonConvert.SerializeObject(new { request.PhoneNumber, Picture = "Picture byteArray removed for brevity" }));

                var savedRequest = await _requestDAO.Find(request.PhoneNumber);
                if (savedRequest == null)
                {
                    _logger.LogInformation(" Call to _requestDAO.Find() Inside the AddPhoto  of the WalletOpeningService at {0} Response:====>{1}", DateTime.UtcNow, JsonConvert.SerializeObject(savedRequest));

                    response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.REQUEST_NOT_FOUND,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language)
                    };
                    return response;
                }
                var suctomerExits = await _customerDAO.FindByWalletNumber(request.PhoneNumber);
                //Mutted to allow change of User photo
                //if ((savedRequest != null && savedRequest.Status == WalletOpeningStatus.COMPLETED) && (suctomerExits != null && suctomerExits.HasWallet == true))
                //{
                //    _logger.LogInformation("Response from  _customerDAO.FindByWalletNumber User has already onboarded: response _requestDAO.Find()====>{0}.  Response From  _customerDAO.FindByWalletNumber()  =====> {1}", JsonConvert.SerializeObject(savedRequest), JsonConvert.SerializeObject(suctomerExits));

                //    response.FaultType = FaultMode.EXISTS;
                //    response.Error = new ErrorResponse
                //    {
                //        ResponseCode = ResponseCodes.WALLET_ALREADY_OPENED|||||||||||||
                //        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.WALLET_ALREADY_OPENED, language)
                //    };
                //    return response;
                //}
                
                var fileName = $"{request.PhoneNumber}.{_settings.ImageFormat}";

                imagePath = await _imageManager.SaveImage(request.PhoneNumber, fileName, DocumentType.PICTURE, request.Picture);

                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    _logger.LogInformation(" Image path is null");


                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.IMAGE_PROFILE_NOT_CREATED,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.IMAGE_PROFILE_NOT_CREATED, language)
                    };
                    return response;
                }

                if (string.IsNullOrEmpty(imagePath))
                {
                    response.FaultType = FaultMode.INVALID_OBJECT_STATE;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.IMAGE_UNPROCCESSABLE,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.IMAGE_UNPROCCESSABLE, language)
                    };
                    return response;
                }
                response.IsSuccessful = true;
                if ((savedRequest != null && savedRequest.Status == WalletOpeningStatus.COMPLETED) || (suctomerExits != null && suctomerExits.HasWallet == true))
                {
                    savedRequest.Status = WalletOpeningStatus.COMPLETED;
                }
                else
                {
                    savedRequest.Status = WalletOpeningStatus.PHOTO_PROVIDED;
                }
                savedRequest.PhotoLocation = imagePath;


                await _requestDAO.Update(savedRequest);
                //TODO: Add try-catch to cleanup image if update fails
                var photourl = _imageManager.GetFileViewUrl(imagePath, _settings.PhotoReturnedBaseUrl);
                response.FaultType = FaultMode.NONE;
                response.SetPayload(new PhotoUploadResponse { PhotoUrl = photourl, IsSuccessful = true, WalletNumber = request.PhoneNumber });
                return response;
            }
            catch (Exception ex)
            {


                _logger.LogCritical(ex, "Error occurred in the AddIdentificationDocument of the WalletOpeningService at {0}", DateTime.UtcNow);
                await _imageManager.DeletFileFromFileDirectory(imagePath);
                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("network-related or instance-specific error"))
                {
                    return new ServiceResponse<PhotoUploadResponse>
                    {
                        Error = new ErrorResponse { ResponseCode = ResponseCodes.SQL_DATABASE_NETWORK_ERROR, ResponseDescription = _messageProvider.GetMessage(ResponseCodes.SQL_DATABASE_NETWORK_ERROR, language) }
                 ,
                        FaultType = FaultMode.SERVER,
                        IsSuccessful = false
                    };

                }
                return new ServiceResponse<PhotoUploadResponse>
                {
                    Error = new ErrorResponse { ResponseCode = ResponseCodes.GENERAL_ERROR, ResponseDescription = ex.Message }
                ,
                    FaultType = FaultMode.SERVER,
                    IsSuccessful = false
                };

            }
        }


        public async Task<ServiceResponse<WalletCompletionResponse>> Complete(WalletCompletionRequest request, string language)
        {
            _logger.LogInformation("Inside the Complete method of the Wallet Opening Service  at {0}", DateTime.UtcNow);
            if (!request.IsValid(out var source))
            {
                return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.CLIENT_INVALID_ARGUMENT,
                       ResponseCodes.INVALID_INPUT_PARAMETER,
                       $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}");
            }

            var item = await _requestDAO.Find(request.PhoneNumber);

            if (item == null)
            {
                return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                    ResponseCodes.REQUEST_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language));
            }
            _logger.LogInformation("calling the _customerDAO.FindByWalletNumber in the conplete method of Walletopeingservice. Request ======>{0}", request.PhoneNumber);
            var customerRecordSaved = await _customerDAO.FindByWalletNumber(request.PhoneNumber);

            long custId = customerRecordSaved != null ? customerRecordSaved.Id : 0;
            var deviceSaved = await _deviceDAO.Find(request.DeviceId);
            long deviceCustomerId = deviceSaved != null ? Convert.ToInt64(deviceSaved.Customer_Id) : 0;
            var documentExists = _documentDAO.FindByWalletNumber(request.PhoneNumber)
                                                .GetAwaiter()
                                                .GetResult()
                                                .FirstOrDefault();


            if ((custId != 0) && (custId == deviceCustomerId) && (documentExists != null))
            {
                if (item.Status == WalletOpeningStatus.COMPLETED)
                {
                    //Check if customer already has a profile. If not, activate the profile
                    return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.EXISTS,
                        ResponseCodes.WALLET_ALREADY_OPENED, _messageProvider.GetMessage(ResponseCodes.WALLET_ALREADY_OPENED, language));
                }
            }

            //if (!(await _deviceDAO.IsAvailable(request.DeviceId)))
            //{
            //    return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.INVALID_OBJECT_STATE,
            //        ResponseCodes.DEVICE_NOT_AVAILABLE, _messageProvider.GetMessage(ResponseCodes.DEVICE_NOT_AVAILABLE, language));
            //}

            
            //if (deviceSaved != null && deviceCustomerId > 0)
            //{
            //    return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.INVALID_OBJECT_STATE,
            //        ResponseCodes.DEVICE_NOT_AVAILABLE, _messageProvider.GetMessage(ResponseCodes.DEVICE_NOT_AVAILABLE, language));
            //}
            _logger.LogInformation("Calling the otp.find method in from the wallet opening service at {0} :Request===>{1}", DateTime.UtcNow, JsonConvert.SerializeObject(new { request.PhoneNumber, OtpPurpose.WALLET_OPENING }));
            var otp = await _otpDAO.Find(request.PhoneNumber, OtpPurpose.WALLET_OPENING);
            _logger.LogInformation("otp response:=====> {0} in the Complete method of the walletopening service at {1} ", JsonConvert.SerializeObject(otp), DateTime.UtcNow);
            if (otp == null)
            {
                return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.UNAUTHORIZED, ResponseCodes.CODE_VALIDATION_ERROR, _messageProvider.GetMessage(ResponseCodes.CODE_VALIDATION_ERROR, language));
            }
            _logger.LogInformation("Calling the otp.DateCreated.AddMinutes for  the otp duration check in the complete method of wallet opening service at {0} ", DateTime.UtcNow);

            if (_settings.IsTest == false)
            {
                if (DateTime.UtcNow > otp.DateCreated.AddMinutes(_settings.OtpDuration))
                {
                    _logger.LogInformation("Duration of otp has elapsed after {0} of sending the otp to you. in the complete method  ", _settings.OtpDuration, DateTime.UtcNow);

                    return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.UNAUTHORIZED, ResponseCodes.CODE_VALIDATION_ERROR,
                                                               _messageProvider.GetMessage(ResponseCodes.CODE_VALIDATION_ERROR, language));
                }
            }



            _logger.LogInformation("Comparing  the   entered otp with the one saved to database with salt complete method of the wallet opening service at {0} ", DateTime.UtcNow);
            if (!_cryptoService.AreEqual(request.Otp, otp.Code, otp.Salt))
            {
                return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.UNAUTHORIZED, ResponseCodes.CODE_VALIDATION_ERROR,
                    _messageProvider.GetMessage(ResponseCodes.CODE_VALIDATION_ERROR, language));
            }



            var response = new ServiceResponse<WalletCompletionResponse>();


            var profileManagerResponse = await _profileManager.ActivateProfile(
                item.PhoneNumber, request.TransactionPin, request.Password, request.SecretQuestions);
            var dca = string.Empty;
            if (profileManagerResponse != null) { dca = JsonConvert.SerializeObject(profileManagerResponse); }
            _logger.LogInformation(" active profile response {0} in the Complete method of the walletopening service ", dca);
            if (profileManagerResponse != null)
            {
                if (!profileManagerResponse.IsSuccessful)
                {
                    response.IsSuccessful = false;
                    response.FaultType = profileManagerResponse.FaultType;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = profileManagerResponse.Error.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(profileManagerResponse.Error.ResponseCode, language)
                    };
                    return response;
                }
            }
            _logger.LogInformation("Started  generating the Referralcode in the Complete method of Walletopening service");
            var refCode = await GetUniqueReferralCode();
            _logger.LogInformation("Finished generating the Referralcode in the Complete method of Walletopening service: referralcode:======>{0}", refCode);
            //Not imp;emented on the deployed App on production: Jsut new

            var refExist = await ReferralCodeExists(request.ReferralCode, language);
            var refBy = refExist.Exists == true ? request.ReferralCode : "";
            var customer = new Customer
            {
                DateCreated = DateTime.UtcNow,
                EmailAddress = item.EmailAddress,
                FirstName = item.FirstName,
                IsActive = true,
                LastName = item.LastName,
                MiddleName = item.MiddleName,
                WalletNumber = item.PhoneNumber,
                OnboardingStatus = OnboardingStatus.COMPLETED,
                HasWallet = true,
                Gender = item.Gender,
                Title = item.Salutation,
                ReferredBy = refBy,
 
 
                ReferralCode = refCode, OnboardingType = Core.Enums.OnboardingType.Wallet
 
            };

            var ticket = _customerDAO.Begin();
            _deviceDAO.Join(ticket);
            _otpDAO.Join(ticket);
            _caseDAO.Join(ticket);
            _documentDAO.Join(ticket);
            _requestDAO.Join(ticket);
            _customerDAO.Join(ticket);

            try
            {
                long customerId = 0;
                _logger.LogInformation("Calling the customerDAO.add  method of the CustomerDAO in from the wallet opening service at {0} ", DateTime.UtcNow);
                if (customerRecordSaved == null)
                {

                    _logger.LogInformation("Started  calling  the _customerDAO.Add in the Complete method of Walletopening service: Request:====.{0}", JsonConvert.SerializeObject(customer));

                    customerRecordSaved = await _customerDAO.Add(customer);
                    _logger.LogInformation("Finished  calling  the _customerDAO.Add in the Complete method of Walletopening service: Response:====.{0}", JsonConvert.SerializeObject(customerId));


                    if (customerRecordSaved != null)
                    {
                        customerId = customerRecordSaved.Id;
                    }
                    else
                    {
                        _logger.LogInformation("Calling  of the _customerDAO.Add  method in the Complete method of Walletopening service: responded with:====.{0}; meaning , it falied", JsonConvert.SerializeObject(customerId));

                        response.Error.ResponseDescription = _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_REGISTERED, language);
                        return response;

                    }

                }
                else
                {

                    customerId = customerRecordSaved.Id;
                }



                var device = new Device
                {
                    DateCreated = DateTime.UtcNow,
                    DeviceId = request.DeviceId,
                    IsActive = true,
                    Customer_Id = customerId,
                    Model = request.DeviceModel
                };

                //deviceSaved  customerRecordSaved  documentExists
                if (deviceSaved == null)
                {
                    _logger.LogInformation("Calling the DeviceDAO.add  method of the DeviceDAO from the wallet opening service at {0} ", DateTime.UtcNow);
                    var devSaveResult = await _deviceDAO.Add(device);
                    var dcw = string.Empty;
                    if (devSaveResult != null) { dcw = JsonConvert.SerializeObject(devSaveResult); }
                    _logger.LogInformation("device   response {0} in the Complete method of the walletopening service at", dcw);
                    if (devSaveResult != "saved")
                    {

                        response.Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.DEVICE_REGISTRATION_FAILURE,
                            ResponseDescription =
                                _messageProvider.GetMessage(ResponseCodes.DEVICE_REGISTRATION_FAILURE, language)
                        };
                        return response;
                    }

                }
                else
                {
                    if (deviceSaved.Customer_Id != customerId)
                    {
                        return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.INVALID_OBJECT_STATE,
                            ResponseCodes.NEW_DEVICE_DETECTED, _messageProvider.GetMessage(ResponseCodes.NEW_DEVICE_DETECTED, language));
                    }
                    if (deviceSaved.IsActive == false)
                    {
                        return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.INVALID_OBJECT_STATE,
                            ResponseCodes.DEVICE_DISABLED, _messageProvider.GetMessage(ResponseCodes.DEVICE_DISABLED, language));
                    }

                    if (deviceSaved.Customer_Id == null)
                    {
                        deviceSaved.Customer_Id = customerId;
                        await _deviceDAO.Update(device);
                    }
                }
                //if (!(await _deviceDAO.IsAvailable(request.DeviceId)))
                //{
                //    return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.INVALID_OBJECT_STATE,
                //        ResponseCodes.DEVICE_NOT_AVAILABLE, _messageProvider.GetMessage(ResponseCodes.DEVICE_NOT_AVAILABLE, language));
                //}

                 

                var newCase = new Case
                {
                    RequestReference = Guid.NewGuid().ToString(),
                    Customer_Id = customerId,
                    State = CaseState.NEW,
                    AccountId = item.PhoneNumber,
                    AccountType = AccountType.WALLET,
                    DateOfBirth = item.BirthDate
                };
                _logger.LogInformation("Calling the CaseDAO.add  method of the CaseDAO  from the wallet opening service at {0} ", DateTime.UtcNow);

                var caseId = await _caseDAO.Add(newCase);
                _logger.LogInformation("CaseDOA response {0} in the Complete method of the walletopening service ", caseId);

                var document = new Document
                {
                    Location = item.PhotoLocation.Replace("\u200B", "").Trim(),
                    State = DocumentState.NEW,
                    Type = DocumentType.PICTURE,
                    Reference = item.BiometricTag.Replace("\u200B", "").Trim(),
                    Case_Id = caseId,
                    LastUpdateDate = DateTime.Now.Date,
                    PhoneNumber = request.PhoneNumber.Replace("\u200B", "").Trim(),
                    Status = DocumentStatus.PENDING,
                    CustomerId = customerId
                };

                if (documentExists == null)
                {
                    _logger.LogInformation("Calling the DocumentDAO.add  method of the  DocumentDAO from the wallet opening service at {0} ", DateTime.UtcNow);

                    var documentSaved = await _documentDAO.Add(document);

                    var documentSavedJson = JsonConvert.SerializeObject(documentSaved);
                    _logger.LogInformation($"Document DAO response {documentSavedJson} in the Complete method of the walletopening service at {0} ", DateTime.UtcNow);
                    if (documentSaved == null)
                    {
                        response.Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.DOCUMENT_FILING_FAILURE,
                            ResponseDescription =
                                _messageProvider.GetMessage(ResponseCodes.DOCUMENT_FILING_FAILURE, language)
                        };
                        return response;

                    }


                }

                ///Create the wallet from wallet service

                var payload = new WalletCreationRequest
                {
                    PhoneNumber = request.PhoneNumber,
                    Email = item.EmailAddress,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    MiddleName = item.MiddleName,
                    BirthDate = item.BirthDate.ToString("dd-MM-yyyy"),
                    Gender = item.Gender,
                    Nationality = item.Nationality,
                    Address = item.Nationality,
                    MotherMaidenName = "",
                    State = "",
                    LGA = ""

                };

                if (item != null)
                {
                    _logger.LogInformation("calling the  _walletCreator.OpenWallet in the conplete method of Walletopeingservice. Request:======>{0}", JsonConvert.SerializeObject(payload));
                    response = await _walletCreator.OpenWallet(payload);
                    _logger.LogInformation("Finished calling the  _walletCreator.OpenWallet in the conplete method of Walletopeingservice. Resoonse:======>{0}", JsonConvert.SerializeObject(response));

                    if (response.IsSuccessful == false)
                    {

                        if (response.Error.ResponseCode == "WE04")
                        {  //Update the WalletOpeningRequest record
                            item.Status = WalletOpeningStatus.COMPLETED;
                            _logger.LogInformation("Started Calling the otp.update method in from the wallet opening service at {0} . Request:======>{1}", DateTime.UtcNow, JsonConvert.SerializeObject(item));
                            await _requestDAO.Update(item);

                            await _otpDAO.Delete(otp.Id);
                            response.IsSuccessful = true;
                            response.Error = null;
                            ticket.Commit();
                        }
                        else
                        {
                            response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                        }


                        return response;


                    }
                    else
                    {

                        //Update the WalletOpeningRequest record
                        item.Status = WalletOpeningStatus.COMPLETED;
                        _logger.LogInformation("Started Calling the otp.update method in from the wallet opening service at {0} . Request:======>{1}", DateTime.UtcNow, JsonConvert.SerializeObject(item));
                        await _requestDAO.Update(item);

                        await _otpDAO.Delete(otp.Id);

                        ticket.Commit();
                    }
                    var dcow = string.Empty;
                    if (response != null) { dcow = JsonConvert.SerializeObject(response); }
                    _logger.LogInformation("wallet opening creation response {0} in the Complete method of the walletopening service ", dcow);
                    if (response != null)
                    {
                        if (!response.IsSuccessful)
                        {
                            response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                            return response;
                        }
                    }

                }



            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Error completing wallet opening request for {0}", request.PhoneNumber);
                ticket.Rollback();
                if (!string.IsNullOrWhiteSpace(e.Message) && e.Message.Contains("network-related or instance-specific error"))
                {
                    response.Error = new ErrorResponse { ResponseCode = ResponseCodes.SQL_DATABASE_NETWORK_ERROR, ResponseDescription = _messageProvider.GetMessage(ResponseCodes.SQL_DATABASE_NETWORK_ERROR, language) };
                    return response;
                }
                return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.SERVER,
                        ResponseCodes.GENERAL_ERROR, _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language));
            }

            response.IsSuccessful = true;
            //TODO: Notify the biometric service
            var dcl = string.Empty;
            response.IsSuccessful = true;
            response.FaultType = FaultMode.NONE;



            if (response != null) { dcl = JsonConvert.SerializeObject(response); }
            _logger.LogInformation("Record after wallet opening {0}", dcl);
            return response;
        }

        public async Task<BasicResponse> CreateOTP(string phoneNumber, string language)
        {
            try
            {


                var create = await _otpService.CreateOtpMessage(phoneNumber, language, OtpPurpose.WALLET_OPENING);
                if (!string.IsNullOrWhiteSpace(create))
                {
                    return new BasicResponse { IsSuccessful = true };
                }
                return new BasicResponse { IsSuccessful = false };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "server error occurred in the CreateOTP method of WalletOpening at {0}", DateTime.UtcNow); ;

                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("network-related or instance-specific error"))
                {
                    return new BasicResponse { Error = new ErrorResponse { ResponseCode = ResponseCodes.SQL_DATABASE_NETWORK_ERROR, ResponseDescription = _messageProvider.GetMessage(ResponseCodes.SQL_DATABASE_NETWORK_ERROR, language) } };

                }
                return new BasicResponse { IsSuccessful = false };
            }
        }

        public async Task<ServiceResponse<WalletStatus>> GetWalletOpeningStatus(string phoneNumber, string language)
        {
            var response = new ServiceResponse<WalletStatus>(false);
            try
            {
                _logger.LogInformation("inside the  GetWalletOpeningStatus  method of WalletOPeningService class {0}", DateTime.UtcNow);


                _logger.LogInformation(" Checking if the  walletNumber has already exists in the  GetWalletOpeningStatus    method ofWalletOPeningService class at {0}", DateTime.UtcNow);

                var request = await _requestDAO.Find(phoneNumber);
                var customerdetail = await _customerDAO.FindByWalletNumber(phoneNumber);
                long id = customerdetail != null ? customerdetail.Id : 0;
                _logger.LogInformation("Calling of _deviceDAO.FindByCustomerId() in the  GetWalletOpeningStatus  method of WalletOPeningService class {0} returned null", DateTime.UtcNow);

                var dev = await _deviceDAO.FindByCustomerId(id);
                _logger.LogInformation("Finished calling  of _deviceDAO.FindByCustomerId() in the  GetWalletOpeningStatus  method of WalletOPeningService class {0} to check if the device already exists . Response=====>{1}", DateTime.UtcNow, dev);


                if (request == null)
                {

                    // var userAlreadyInTheWalletOpeingRequestsTable=await   _requestDAO.Find(phoneNumber);




                    _logger.LogInformation("Calling of _deviceDAO.FindByCustomerId() in the  GetWalletOpeningStatus  method of WalletOPeningService class {0} returned null", DateTime.UtcNow);


                    response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.PROFILE_NOT_FOUND,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.PROFILE_NOT_FOUND, language)
                    };
                    return response;
                }
                _logger.LogInformation(" Checking if the  wallet has already been completed  in the  GetWalletOpeningStatus    method ofWalletOPeningService class at {0}", DateTime.UtcNow);

                if ((request != null && request.Status == WalletOpeningStatus.COMPLETED) && (customerdetail != null && customerdetail.HasWallet == true))
                {

                    response.FaultType = FaultMode.EXISTS;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.CUSTOMER_ALREADY_ONBOARDED,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.CUSTOMER_ALREADY_ONBOARDED, language)
                    };
                    response.SetPayload(new WalletStatus
                    {
                        PhoneNumber = phoneNumber,
                        Status = request.Status,
                        BiometricTag = request.BiometricTag,
                        BirthDate = request.BirthDate,
                        EmailAddress = request.EmailAddress,
                        FirstName = request.FirstName,
                        MiddleName = request.MiddleName,
                        LastName = request.LastName,
                        Gender = request.Gender,
                        IdNumber = request.IdNumber,
                        IdType = request.IdType,
                        Nationality = request.Nationality,
                        Salutation = request.Salutation
                    });
                    return response;
                }

                var status = new WalletStatus
                {
                    PhoneNumber = phoneNumber,
                    Status = request.Status,
                    BiometricTag = request.BiometricTag,
                    BirthDate = request.BirthDate,
                    EmailAddress = request.EmailAddress,
                    FirstName = request.FirstName,
                    MiddleName = request.MiddleName,
                    LastName = request.LastName,
                    Gender = request.Gender,
                    IdNumber = request.IdNumber,
                    IdType = request.IdType,
                    Nationality = request.Nationality,
                    Salutation = request.Salutation
                };
                _logger.LogInformation(" GetWalletOpeningStatus  in the  ofWalletOPeningService class at {0} Response:===>{1}", DateTime.UtcNow, JsonConvert.SerializeObject(status));


                response.IsSuccessful = true;
                response.SetPayload(status);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A critical Server error has occurred in the   GetWalletOpeningStatus   method ofWalletOPeningService class at {0}", DateTime.UtcNow);

                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("network-related or instance-specific error"))
                {
                    response.Error = new ErrorResponse { ResponseCode = ResponseCodes.SQL_DATABASE_NETWORK_ERROR, ResponseDescription = _messageProvider.GetMessage(ResponseCodes.SQL_DATABASE_NETWORK_ERROR, language) };
                    return response;
                }
                response.FaultType = FaultMode.INVALID_OBJECT_STATE;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.REQUEST_NOT_COMPLETED,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_COMPLETED, language)
                };
                return response;
            }
        }

        public async Task<BasicResponse> PhoneNumberAlreadyOnboarded(string phoneNumber)
        {
            var isOonboardedBefore = await _customerDAO.FindByWalletNumber(phoneNumber);

            if (isOonboardedBefore != null)
            {
                return new BasicResponse { FaultType = FaultMode.NONE, IsSuccessful = true };
            }
            return new BasicResponse { FaultType = FaultMode.NONE, IsSuccessful = false };

        }

        private bool ReferralcodeExists(string referredBy)
        {
            var response = _customerDAO.ReferralCodeExists(referredBy).GetAwaiter().GetResult();
            if (!string.IsNullOrWhiteSpace(response))
            {
                return true;
            }

            return false;
        }
        public async Task<ServiceResponse<WalletInitialisationResponse>> InitialiseWallet(WalletInitialisationRequest request, string language)
        {
            _logger.LogInformation("inside the InitialiseWallet  method of WalletOPeningService class");
            var response = new ServiceResponse<WalletInitialisationResponse>(false);


            try
            {

                var isonboardedBefore = await PhoneNumberAlreadyOnboarded(request.PhoneNumber);
                if (isonboardedBefore.IsSuccessful == true)
                {

                    return ErrorResponse.Create<ServiceResponse<WalletInitialisationResponse>>(FaultMode.EXISTS,
                     ResponseCodes.PHONENUMBER_ALREADY_EXISTS, _messageProvider.GetMessage(ResponseCodes.PHONENUMBER_ALREADY_EXISTS, language));
                }

                request.EmailAddress = request.EmailAddress.Trim().ToLower();
                request.PhoneNumber = request.PhoneNumber.Trim();


                _logger.LogInformation("Calling _deviceDAO.IsAvailable inside  InitialiseWallet  method of WalletOPeningService class");

                if (!(await _deviceDAO.IsAvailable(request.DeviceId)))
                {
                    _logger.LogInformation("Logging error  for the   _deviceDAO.IsAvailable in theInitialiseWallet  method of WalletOPeningService class");
                    return ErrorResponse.Create<ServiceResponse<WalletInitialisationResponse>>(FaultMode.INVALID_OBJECT_STATE,
                        ResponseCodes.DEVICE_NOT_AVAILABLE, _messageProvider.GetMessage(ResponseCodes.DEVICE_NOT_AVAILABLE, language));
                }
                _logger.LogInformation("Calling the   _customerDAO.Exists   in theInitialiseWallet  method of WalletOPeningService class : request ===> {0}", request.PhoneNumber);

                if (await _customerDAO.Exists(request.PhoneNumber))
                {
                    _logger.LogInformation("Finished calling the   _customerDAO.Exists   in theInitialiseWallet  method of WalletOPeningService class : response ===>_{0}", "Customer exists");


                    response.FaultType = FaultMode.INVALID_OBJECT_STATE;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.CUSTOMER_ALREADY_ONBOARDED,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.CUSTOMER_ALREADY_ONBOARDED, language)
                    };
                    return response;
                }
                _logger.LogInformation("Calling the  _requestDAO.Find  in theInitialiseWallet  method of WalletOPeningService class : request ===>{0}", request.PhoneNumber);

                var item = await _requestDAO.Find(request.PhoneNumber);
                if (item != null)
                {
                    _logger.LogInformation("The wallet number already exists in the WalletOpeingRequests table  in the WalletOPeningService class request ===>{0}", request.PhoneNumber);


                    response.FaultType = FaultMode.EXISTS;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.PLEASE_TRY_RESUMING_ONBORADING,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.PLEASE_TRY_RESUMING_ONBORADING, language)
                    };

                    return response;
                }
                _logger.LogInformation("Calling the  _profileManager.ReserveProfile in theInitialiseWallet  method of WalletOPeningService class : request ===>_{0}", request.PhoneNumber);


                var rsp = await _profileManager.ReserveProfile(request.EmailAddress, request.PhoneNumber,
                    request.FirstName, request.LastName, request.MiddleName);
                if (!rsp.IsSuccessful)
                {
                    _logger.LogInformation("call to _profileManager.ReserveProfile in the WalletOPeningService class : request ===>_{0} returned:====>{1}", request.PhoneNumber, rsp.IsSuccessful);


                    response.FaultType = rsp.FaultType;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = rsp.Error.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(rsp.Error.ResponseCode, language)
                    };
                    return response;
                }

                var biometricTag = Guid.NewGuid().ToString();
                var entity = new WalletOpeningRequest
                {
                    BiometricTag = biometricTag,
                    BirthDate = request.BirthDate,
                    EmailAddress = request.EmailAddress,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Gender = request.Gender,
                    MiddleName = request.MiddleName,
                    PhoneNumber = request.PhoneNumber,
                    Status = WalletOpeningStatus.INITIALISED,
                    Salutation = request.Salutation,
                    DateCreated = DateTime.Now
                };
                _logger.LogInformation(" Start Saving {0} into the WalletOpeingRequests table in the WalletOPeningService class {1}", JsonConvert.SerializeObject(entity), DateTime.UtcNow);

                var result = await _requestDAO.Add(entity);
                _logger.LogInformation(" Finished Saving  into the WalletOpeingRequests table in the WalletOPeningService class {0}. Response:=======>{1}. returned data to user: {2}=====>", DateTime.UtcNow, JsonConvert.SerializeObject(result), biometricTag);


                if (result > 0)
                {


                    response.IsSuccessful = true;
                    response.SetPayload(new WalletInitialisationResponse
                    {
                        BiometricReference = biometricTag
                    });
                }

            }
            catch (Exception e)
            {
                _logger.LogCritical(e, " An error occurred in the InitialiseWallet of the WalletOPeningService ");


                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };
                return response;

            }


            return response;


        }

        public async Task<BasicResponse> SendValidationCode(string phoneNumber, string language)
        {
            try
            {
                _logger.LogInformation("Inside the SendValidationCode method of the  WalletOpeningService at {0}", DateTime.UtcNow);
                var response = new BasicResponse(false);

                var item = await _requestDAO.Find(phoneNumber);


                if (item == null)
                {
                    _logger.LogInformation(" Requst not found after calling _requestDAO.Find( method Inside the SendValidationCode method of the  WalletOpeningService at {0}, Response====>{1}", DateTime.UtcNow, JsonConvert.SerializeObject(item));

                    response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.REQUEST_NOT_FOUND,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language)

                    };
                    return response;
                }

                //if (_settings.IsTest != true)
                //{
                //    if (item.Status != WalletOpeningStatus.PHOTO_PROVIDED)
                //    {
                //        response.FaultType = FaultMode.PHOTO_NOT_YET_PROVIDED;
                //        response.Error = new ErrorResponse
                //        {
                //            ResponseCode = ResponseCodes.INVALID_WALLET_REGISTRATION_STATE,
                //            ResponseDescription = _messageProvider.GetMessage(ResponseCodes.PHOTO_NOT_YET_PROVIDED, language)

                //        };
                //        return response;

                //    }
                //}

                _logger.LogInformation("Creating OTP--------- Request:=======>{0}", JsonConvert.SerializeObject(new { phoneNumber, language, OtpPurpose.WALLET_OPENING }));
                var message = await _otpService.CreateOtpMessage(phoneNumber, language, OtpPurpose.WALLET_OPENING);
                _logger.LogInformation("Finished Creating OTP--------- Response:=======>{0}", JsonConvert.SerializeObject(message));

                _ = _notifier.SendSMS("9999999999", item.PhoneNumber, message);
                response.IsSuccessful = true;
                return response;
            }
            catch (Exception ex)
            {


                _logger.LogCritical(ex, "Error occurred in the SendValidationCode  of the WalletOpeningService   at {0}", DateTime.UtcNow);

                _logger.LogCritical(ex, "Error occurred in the UpdatePersonalInformation of the  Wallet Openeing Service at {0}", DateTime.UtcNow);

                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("network-related or instance-specific error"))
                {
                    return new BasicResponse { Error = new ErrorResponse { ResponseCode = ResponseCodes.SQL_DATABASE_NETWORK_ERROR, ResponseDescription = _messageProvider.GetMessage(ResponseCodes.SQL_DATABASE_NETWORK_ERROR, language) } };

                }

                return new BasicResponse { Error = new ErrorResponse { ResponseCode = ResponseCodes.GENERAL_ERROR, ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language) } };

            }

        }

        public async Task<BasicResponse> UpdatePersonalInformation(string phoneNumber, Biodata data, string language)
        {
            try
            {
                _logger.LogInformation("Inside the UpdatePersonalInformation of the WalletOpeningService at {0}", DateTime.UtcNow);
                var request = await _requestDAO.Find(phoneNumber);
                if (request == null)
                {
                    return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                            ResponseCodes.REQUEST_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language));
                }
                if (request.Status == WalletOpeningStatus.COMPLETED)
                {
                    return ErrorResponse.Create<ServiceResponse<WalletCompletionResponse>>(FaultMode.INVALID_OBJECT_STATE,
                        ResponseCodes.WALLET_ALREADY_OPENED, _messageProvider.GetMessage(ResponseCodes.WALLET_ALREADY_OPENED, language));
                }

                if (!string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName != "string")
                {
                    request.FirstName = data.FirstName;
                }
                if (!string.IsNullOrWhiteSpace(request.MiddleName))
                {
                    request.MiddleName = data.MiddleName;
                }
                if (!string.IsNullOrWhiteSpace(request.LastName))
                {
                    request.LastName = data.LastName;
                }
                if (request.BirthDate != default(DateTime) && request.BirthDate != DateTime.Now.Date)
                {
                    request.BirthDate = data.BirthDate;
                }
                if (!string.IsNullOrWhiteSpace(request.Gender))
                {
                    request.Gender = data.Gender;
                }
                if (!string.IsNullOrWhiteSpace(request.Salutation))
                {
                    request.Salutation = data.Salutation;
                }


                await _requestDAO.Update(request);
                return new BasicResponse(true);
            }
            catch (Exception ex)
            {


                _logger.LogCritical(ex, "Error occurred in the UpdatePersonalInformation of the  Wallet Openeing Service at {0}", DateTime.UtcNow);
                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("network-related or instance-specific error"))
                {
                    return new BasicResponse { Error = new ErrorResponse { ResponseCode = ResponseCodes.SQL_DATABASE_NETWORK_ERROR, ResponseDescription = _messageProvider.GetMessage(ResponseCodes.SQL_DATABASE_NETWORK_ERROR, language) } };

                }
                return new BasicResponse { Error = new ErrorResponse { ResponseCode = ResponseCodes.GENERAL_ERROR, ResponseDescription = "Server Error occurred" } };
            }

        }

        public async Task<string> GetUniqueReferralCode()
        {
            try
            {
                var refCode = _codeGenerator.ReferralCode(_settings.CodeLength);
                var refeeralCodeExists = await _customerDAO.FindByReferralCode(refCode);
                while (refeeralCodeExists == true)
                {
                    refCode = _codeGenerator.ReferralCode(_settings.CodeLength);
                    refeeralCodeExists = await _customerDAO.FindByReferralCode(refCode);
                }
                return refCode;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Server error occurred in the GetUniqueReferralCode of Walletopeingservive");
                return null;
            }


        }

        public async Task<ReferralCodeResponse> ReferralCodeExists(string referralCode, string language)
        {
            var response = new ReferralCodeResponse(true);
            string refByExists = await _customerDAO.ReferralCodeExists(referralCode);

            if (string.IsNullOrWhiteSpace(refByExists))
            {
                response.Exists = false;
                response.Message = _messageProvider.GetMessage(ResponseCodes.REFERRALCODE_DOES_NOT_EXISTS, language);

            }

            return response;
        }


    }
}
