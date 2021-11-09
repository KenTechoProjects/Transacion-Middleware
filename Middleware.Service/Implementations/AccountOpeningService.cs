using Middleware.Service.DTOs;
using Middleware.Service.FIServices;
using Middleware.Service.Processors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Middleware.Core.DTO;

using Middleware.Core.DAO;
using Middleware.Service.DAO;
using AutoMapper;
using Middleware.Core.Model;
using Middleware.Service.Utilities;
using System.Linq;
//using Document = Middleware.Core.Model.Document;
using Middleware.Core.Enums;
using Document = Middleware.Core.Model.Document;

namespace Middleware.Service.Implementations
{
    //NOT DONE YET
    public class AccountOpeningService : IAccountOpeningService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly AccountOpeningSettings _accountOpeningSettings;
        private readonly SystemSettings _settings;
        const string APP_ID = "AppId";
        const string APP_KEY = "AppKey";
        const string MEDIA_TYPE = "application/json";
        const string SUCCESS_RESPONSE = "00";
        const string ACCOUNT_OPENING_PREFIX = "AOP";
        const string PND_REMOVAL_PREFIX = "PND";
        private readonly ICurrencyCode _currencyCode;
        private readonly IDocumentDAO _documentDAO;
        private readonly IOtpDAO _otpDAO;
        private readonly IDeviceDAO _deviceDAO;
        private readonly ICustomerDAO _customerDAO;
        private readonly ICaseDAO _caseDAO;
        private readonly IWalletOpeningRequestDAO _requestDAO;
        private readonly IAccountOpeningRequestDAO _accountDAO;
        private readonly IMapper _mapper;
        private readonly IMessageProvider _messageProvider;
        private readonly ICryptoService _cryptoService;
        private readonly IImageManager _imageManager;
        private readonly IOtpService _otpService;
        private readonly INotifier _notifier;
        private readonly ICodeGenerator _codeGenerator;
        private readonly int DEVICE_COUNT = 0;
        private const string SELFIE_SURFIX = "_PIC";
        private const string ID_SURFIX = "_ID";
        private const string SIGNATURE_SURFIX = "_SIGN";
        private const string UTILITY_BILL_SURFIX = "_UBILL";
        public AccountOpeningService(ILoggerFactory logger, IOptions<AccountOpeningSettings> settingsProvider, IHttpClientFactory clientFactory, IOptions<SystemSettings> settings,
            ICurrencyCode currencyCode, IDocumentDAO documentDAO, ICaseDAO caseDAO, IDeviceDAO deviceDAO, ICustomerDAO customerDAO, IWalletOpeningRequestDAO requestDAO,
            IMapper mapper, IMessageProvider messageProvider, ICryptoService cryptoService, IImageManager imageManager, IOtpService otpService,
            INotifier notifier, ICodeGenerator codeGenerator, IOtpDAO otpDAO, IAccountOpeningRequestDAO accountDAO)
        {
            _logger = logger.CreateLogger(typeof(AccountOpeningService));
            _settings = settings.Value;
            _accountOpeningSettings = settingsProvider.Value;
            _httpClient = clientFactory.CreateClient("HttpMessageHandler");
            _httpClient.BaseAddress = new Uri(_accountOpeningSettings.BaseAddress);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(APP_ID, _accountOpeningSettings.AppId);
            _httpClient.DefaultRequestHeaders.Add(APP_KEY, _accountOpeningSettings.AppKey);
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.ServiceTimeout);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MEDIA_TYPE));
            _currencyCode = currencyCode;
            _documentDAO = documentDAO;
            _deviceDAO = deviceDAO;
            _customerDAO = customerDAO;
            _requestDAO = requestDAO;
            _mapper = mapper;
            _messageProvider = messageProvider;
            _cryptoService = cryptoService;
            _imageManager = imageManager;
            _otpService = otpService;
            _notifier = notifier;
            _codeGenerator = codeGenerator;
            _otpDAO = otpDAO;
            _accountDAO = accountDAO;
            DEVICE_COUNT = _settings.MaxDeviceCount;
            _caseDAO = caseDAO;
        }


        //  public async Task<ServiceResponse<AccountOpeningResponse>> OpenAccount(AccountOpeningRequest request)

        #region Private 
        private async Task<AccountOpeningResponse> OpenAccountFI(AccountOpeningPayload request)
        {
            var response = new AccountOpeningResponse();




            string jsonRequest = JsonConvert.SerializeObject(request);
            _logger.LogInformation("ACCOUNT_OPENING_REQUEST:", jsonRequest);
            var content = new StringContent(jsonRequest, null, MEDIA_TYPE);
            HttpResponseMessage responseMessage = await _httpClient.PostAsync(_accountOpeningSettings.OpenAccountEndpoint, content);

            _logger.LogInformation("ACCOUNT_OPENING_RESPONSE_MESSAGE: ", JsonConvert.SerializeObject(responseMessage));

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception("Service invocation failure");
            }

            var rawResponse = await responseMessage.Content.ReadAsStringAsync();
            _logger.LogInformation($"ACCOUNT_OPENING_RESPONSE:", rawResponse);
            var accountOpeningResponse = JsonConvert.DeserializeObject<AccountOpeningResponse>(rawResponse);



            return response;
        }

        private AccountOpeningPayload AccountOpeningPayloadWithoutWallet(AccountOpeningCompositRequest request)
        {
            var openAccount = new AccountOpeningPayload
            {
                Salutation = request.AccountOpeningRequest.Salutation,
                FirstName = request.AccountOpeningRequest.FirstName,
                LastName = request.AccountOpeningRequest.LastName,
                MiddleName = request.AccountOpeningRequest.MiddleName,
                MobileNumber = request.AccountOpeningRequest.PhoneNumber,
                Email = request.AccountOpeningRequest.Email,
                DateOfBirth = request.AccountOpeningRequest.DateOfBirth.ToString("yyyy-MM-dd"),
                Gender = request.AccountOpeningRequest.Gender,
                HouseNumber = "1",
                StreetName = request.AccountOpeningRequest.Address,
                Address = request.AccountOpeningRequest.Address,
                Country = _currencyCode.ConvertCountry(request.CountryCode),
                State = request.AccountOpeningRequest.State,
                City = request.AccountOpeningRequest.City,
                Region = "",
                PostalCode = "23301",
                Signature = request.AccountOpeningRequest.Signature.RawData,
                MaritalStatus = request.AccountOpeningRequest.MaritalStatus,
                BranchCode = request.AccountOpeningRequest.BranchCode,
                AccountType = "SA301",
                Occupation = "014",
                Nationality = request.AccountOpeningRequest.Nationality,
                IdNumber = request.AccountOpeningRequest.IdNumber,
                IdType = request.AccountOpeningRequest.IdType,
                IdImage = request.IdentificationDTO.RawData,
                PassportPhoto = request.Selfie.RawData,
                CurrenyCode = request.CountryCode,
                RequestId = Guid.NewGuid().ToString(),
                CountryId = request.CountryCode
            };
            return openAccount;
        }
        private AccountOpeningPayload AccountOpeningPayloadWithWallet(Service.Model.AccountOpeningRequest request)
        {
            var openAccount = new AccountOpeningPayload
            {
                Salutation = request.Salutation,
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,
                MobileNumber = request.WalletNumber,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth.ToString("yyyy-MM-dd"),
                Gender = request.Gender,
                HouseNumber = "1",
                StreetName = request.Address,
                Address = request.Address,
                Country = _currencyCode.ConvertCountry(request.CountryCode),
                State = request.State,
                City = request.City,
                Region = "",
                PostalCode = "23301",
                Signature = request.Signature,
                MaritalStatus = request.MaritalStatus,
                BranchCode = request.BranchCode,
                AccountType = "SA301",
                Occupation = "014",
                Nationality = request.Nationality,
                IdNumber = request.IdNumber,
                IdType = request.IdType,
                IdImage = request.IdImage,
                PassportPhoto = request.PassportPhoto,
                CurrenyCode = request.CountryCode,
                RequestId = Guid.NewGuid().ToString(),
                CountryId = request.CountryCode
            };
            return openAccount;
        }
        private Document CreateDocumentPayLoad(DocumentDTO document, DocumentType documentType)
        {
            var payload = new Document();


            if (documentType == DocumentType.IDENTIFICATION)
            {
                payload = new Document
                {
                    CustomerId = !string.IsNullOrWhiteSpace(document.CustomerId) ? Convert.ToInt64(document.CustomerId) : 0,
                    Type = documentType,
                    LastUpdateDate = DateTime.Now,
                    Location = document.Location,
                    PhoneNumber = document.WalletNumber,
                    Reference = document.Reference,
                    DocumentName = document.DocumentName,
                    Status = DocumentStatus.PENDING,
                    State = DocumentState.NEW,
                    StatusDate = DateTime.Now,
                    IdNumber = document.IdNumber,
                    IssuanceDate = document.IssuanceDate,
                    ExpiryDate = document.ExpiryDate

                };
            }
            else
            {
                payload = new Document
                {
                    CustomerId = !string.IsNullOrWhiteSpace(document.CustomerId) ? Convert.ToInt64(document.CustomerId) : 0,
                    Type = documentType,
                    LastUpdateDate = DateTime.Now,
                    Location = document.Location,
                    PhoneNumber = document.WalletNumber,
                    Reference = document.Reference,
                    DocumentName = document.DocumentName,
                    Status = DocumentStatus.PENDING,
                    State = DocumentState.NEW,
                    StatusDate = DateTime.Now

                };
            }
            return payload;

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
        private async Task<Service.Model.AccountOpeningRequest> FeedForOlderRecords(dynamic request)
        {
            var customer = await _customerDAO.FindByWalletNumber(request.PhoneNumber);
            var customerWallet = await _requestDAO.Find(request.PhoneNumber);
            request.PassportPhoto = customerWallet.PhotoLocation;
            request.Nationality = customerWallet.Nationality;
            var mapper = _mapper.Map<Service.Model.AccountOpeningRequest>(request);
            return mapper;
        }
        private static void DocumentFeedOne(AccountOpeningRequestForCustomerWithWallet request, Customer customer, string selfieFileName, string signatureFileName, string IdFileName)
        {

            //Selfie
            request.Selfie.CustomerId = customer.Id.ToString();
            request.Selfie.WalletNumber = request.PhoneNumber;
            request.Selfie.DocumentName = selfieFileName;
            //Signature
            request.SignatureDto.CustomerId = customer.Id.ToString();
            request.SignatureDto.WalletNumber = request.PhoneNumber;
            request.SignatureDto.DocumentName = signatureFileName;

            //Idientification
            request.Identification.CustomerId = customer.Id.ToString();
            request.Identification.WalletNumber = request.PhoneNumber;
            request.Identification.DocumentName = IdFileName;
        }
        private static void DocumentFeedTwo(AccountOpeningCompositRequest request, Customer customer, string selfieFileName, string signatureFileName, string IdFileName)
        {

            //Selfie
            request.Selfie.CustomerId = customer.Id.ToString();
            request.Selfie.WalletNumber = request.Customer.PhoneNumber;
            request.Selfie.DocumentName = selfieFileName;
            //Signature
            request.DocumSignatureent.CustomerId = customer.Id.ToString();
            request.DocumSignatureent.WalletNumber = request.Customer.PhoneNumber;
            request.DocumSignatureent.DocumentName = signatureFileName;

            //Idientification
            request.IdentificationDTO.CustomerId = customer.Id.ToString();
            request.IdentificationDTO.WalletNumber = request.Customer.PhoneNumber;
            request.IdentificationDTO.DocumentName = IdFileName;
        }

        private async Task<string> GetUniqueReferralCode()
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
        private async Task<ServiceResponse<AccountOpeningResponse>> CreateAccountCustomerWithWallet(AccountOpeningRequestForCustomerWithWallet request, string language, ServiceResponse<AccountOpeningResponse> response, Model.AccountOpeningRequest accountRequest)
        {
            var ticket = _requestDAO.Begin();
            _otpDAO.Join(ticket);
            _documentDAO.Join(ticket);
            _requestDAO.Join(ticket);
            _customerDAO.Join(ticket);
            var payload = AccountOpeningPayloadWithWallet(accountRequest);
            var initializeAccountOpening = await _accountDAO.Add(accountRequest);
            if (initializeAccountOpening.Id > 0)
            {
                var selfieDocument = CreateDocumentPayLoad(request.Selfie, DocumentType.PICTURE);

                //Save Selfie
                var getDocument = await _documentDAO.FindByDocumentType(payload.MobileNumber, DocumentType.PICTURE);
                if (getDocument != null)
                {
                    getDocument.Location = selfieDocument.Location;
                    getDocument.LastUpdateDate = DateTime.Now;
                    getDocument.DocumentName = selfieDocument.DocumentName;
                    var updateDocunt = await _documentDAO.UpdateKYC(getDocument, true);
                }
                else
                {
                    var saveSelfie = await _documentDAO.Add(selfieDocument);
                    if (saveSelfie.Id > 0)
                    {
                        accountRequest.AccountOpeningStatus = AccountOpeningStatus.SignaturePending;
                        await _accountDAO.Update(accountRequest);
                    }
                    else
                    {
                        response.Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.ACCOUNT_OPENING_SELFIE_FAILURE,
                            ResponseDescription
                            = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_OPENING_SELFIE_FAILURE, language),
                            AccountOpeningStatus = AccountOpeningStatus.SelfiePending
                        };
                        return response;
                    }
                }



                //Save signature
                var signatureDcument = CreateDocumentPayLoad(request.Selfie, DocumentType.SIGNATURE);

                var saveSignature = await _documentDAO.Add(signatureDcument);
                if (saveSignature.Id > 0)
                {
                    accountRequest.AccountOpeningStatus = AccountOpeningStatus.IdentificationPending;
                    await _accountDAO.Update(accountRequest);
                }
                else
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE,
                        ResponseDescription
                        = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE, language),
                        AccountOpeningStatus = AccountOpeningStatus.SignaturePending
                    };
                    return response;
                }

                //Save Id
                var idDocument = CreateDocumentPayLoad(request.Selfie, DocumentType.IDENTIFICATION);

                var saveId = await _documentDAO.Add(idDocument);
                if (saveId.Id > 0)
                {
                    accountRequest.AccountOpeningStatus = AccountOpeningStatus.AccountOpeningPending;
                    await _accountDAO.Update(accountRequest);
                }
                else
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE,
                        ResponseDescription
                        = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE, language),
                        AccountOpeningStatus = AccountOpeningStatus.IdentificationPending
                    };
                    return response;
                }
                //Open account proper

                var createAccount = await OpenAccountFI(payload);

                if (createAccount.IsSuccessful() == true)
                {
                    accountRequest.AccountOpeningStatus = AccountOpeningStatus.Completed;
                    await _accountDAO.Update(accountRequest);
                }
                else
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE,
                        ResponseDescription
                                                   = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE, language),
                        AccountOpeningStatus = AccountOpeningStatus.AccountOpeningPending
                    };
                    return response;
                }
                response.IsSuccessful = true;
                response.SetPayload(createAccount);
                return response;



            }
            else
            {
                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.ACCOUNT_OPENING_INITILIZATON_FAILURE,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_OPENING_INITILIZATON_FAILURE, language)
                };
                return response;
            }
        }

        private async Task<ServiceResponse<AccountOpeningResponse>> CreateAccountCustomerWithNoWallet(AccountOpeningCompositRequest request, string language, ServiceResponse<AccountOpeningResponse> response,
            Model.AccountOpeningRequest accountRequest, string selfieFileName, string signatureFileName, string IdFileName)
        {
            var ticket = _requestDAO.Begin();
            _otpDAO.Join(ticket);
            _caseDAO.Join(ticket);
            _documentDAO.Join(ticket);
            _requestDAO.Join(ticket);
            _customerDAO.Join(ticket);

            //var payload = AccountOpeningPayloadWithWallet(accountRequest);
            var payload = AccountOpeningPayloadWithoutWallet(request);
            var refCode = await GetUniqueReferralCode();
            var initializeAccountOpening = await _accountDAO.Add(accountRequest);

            if (initializeAccountOpening.Id > 0)
            {

                //Save Selfie
                var selfieDocument = CreateDocumentPayLoad(request.Selfie, DocumentType.PICTURE);

                var saveSelfie = await _documentDAO.Add(selfieDocument);
                if (saveSelfie.Id > 0)
                {
                    accountRequest.AccountOpeningStatus = AccountOpeningStatus.SignaturePending;
                    await _accountDAO.Update(accountRequest);
                }
                else
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.ACCOUNT_OPENING_SELFIE_FAILURE,
                        ResponseDescription
                        = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_OPENING_SELFIE_FAILURE, language),
                        AccountOpeningStatus = AccountOpeningStatus.SelfiePending
                    };
                    return response;
                }


                //LocalCutomerAccountCreation
                var customer = new Customer
                {
                    DateCreated = DateTime.Now,
                    BankId = request.Customer.BankId,
                    EmailAddress = request.Customer.EmailAddress,
                    FirstName = request.Customer.FirstName,
                    Gender = request.Customer.Gender,
                    HasAccount = false,
                    HasWallet = false,
                    IsActive = false,
                    LastName = request.Customer.LastName,
                    MiddleName = request.Customer.MiddleName,
                    PhoneNumber = request.Customer.PhoneNumber,
                    OnboardingStatus = OnboardingStatus.STARTED,
                    Title = request.Customer.Title,
                    ReferralCode = refCode,
                    ReferredBy = request.Customer.ReferredBy,
                    WalletNumber = request.Customer.WalletNumber,
                    OnboardingType = Core.Enums.OnboardingType.Account

                };





                var saveCustomerAcountLcal = await _customerDAO.Add(customer);

                if (saveCustomerAcountLcal.Id > 0)
                {
                    accountRequest.AccountOpeningStatus = AccountOpeningStatus.AccountOpeningPending;
                    await _accountDAO.Update(accountRequest);
                }
                else
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE,
                        ResponseDescription
                        = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE, language),
                        AccountOpeningStatus = AccountOpeningStatus.IdentificationPending
                    };
                    return response;
                }



                var device = await _deviceDAO.FindByCustomerId(customer.Id);

                var newCase = new Case
                {
                    RequestReference = Guid.NewGuid().ToString(),
                    Customer_Id = customer.Id,
                    State = CaseState.NEW,
                    AccountId = customer.PhoneNumber,
                    AccountType = AccountType.BANK,
                    DateOfBirth = request.AccountOpeningRequest.DateOfBirth
                };
                var caseId = await _caseDAO.Add(newCase);
                request.Document.CaseId = caseId;
                request.DocumSignatureent.CaseId = caseId;
                request.Selfie.CaseId = caseId;
                request.IdentificationDTO.CaseId = caseId;
                DocumentFeedTwo(request, customer, selfieFileName, signatureFileName, IdFileName);

                //Save signature
                var signatureDcument = CreateDocumentPayLoad(request.DocumSignatureent, DocumentType.SIGNATURE);

                var saveSignature = await _documentDAO.Add(signatureDcument);
                if (saveSignature.Id > 0)
                {
                    accountRequest.AccountOpeningStatus = AccountOpeningStatus.IdentificationPending;
                    await _accountDAO.Update(accountRequest);
                }
                else
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE,
                        ResponseDescription
                        = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE, language),
                        AccountOpeningStatus = AccountOpeningStatus.SignaturePending
                    };
                    return response;
                }

                //Save Id
                var idDocument = CreateDocumentPayLoad(request.IdentificationDTO, DocumentType.IDENTIFICATION);

                var saveId = await _documentDAO.Add(idDocument);
                if (saveId.Id > 0)
                {
                    accountRequest.AccountOpeningStatus = AccountOpeningStatus.LocalCustomerRecordPending;
                    await _accountDAO.Update(accountRequest);
                }
                else
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE,
                        ResponseDescription
                        = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE, language),
                        AccountOpeningStatus = AccountOpeningStatus.IdentificationPending
                    };
                    return response;
                }

                //Open account proper



                var createAccount = await OpenAccountFI(payload);

                if (createAccount.IsSuccessful() == true)
                {
                    accountRequest.AccountOpeningStatus = AccountOpeningStatus.Completed;
                    await _accountDAO.Update(accountRequest);
                    var customerUpdate = await _customerDAO.FindByWalletNumber(customer.WalletNumber);
                    customerUpdate.HasAccount = true;
                    await _customerDAO.Update(customerUpdate);
                }
                else
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE,
                        ResponseDescription
                                                   = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE, language),
                        AccountOpeningStatus = AccountOpeningStatus.AccountOpeningPending
                    };
                    return response;
                }
                response.IsSuccessful = true;
                response.SetPayload(createAccount);
                return response;
            }
            else
            {
                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.ACCOUNT_OPENING_INITILIZATON_FAILURE,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_OPENING_INITILIZATON_FAILURE, language)
                };
                return response;
            }
        }

        private async Task<ReferralCodeResponse> ReferralCodeExists(string referralCode, string language)
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


        #endregion
        #region Public
        public async Task<BasicResponse> RemovePND(PNDRemovalRequest request)
        {
            var response = new BasicResponse(false);

            string jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, null, MEDIA_TYPE);
            HttpResponseMessage responseMessage = await _httpClient.PostAsync(_accountOpeningSettings.PNDRemovalEndpoint, content);

            _logger.LogInformation($"REMOVE_PND_RESPONSE_MESSAGE:", JsonConvert.SerializeObject(responseMessage));

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception("Service invocation failure");
            }

            var rawResponse = await responseMessage.Content.ReadAsStringAsync();
            _logger.LogInformation("REMOVE_PND_RESPONSE: ", JsonConvert.SerializeObject(rawResponse));
            var removePNDResponse = JsonConvert.DeserializeObject<PNDRemovalResponse>(rawResponse);

            if (!removePNDResponse.IsSuccessful())
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = $"{PND_REMOVAL_PREFIX}{removePNDResponse.ResponseCode}"
                };
                return response;
            }

            response.IsSuccessful = true;
            return response;
        }
        public async Task<BasicResponse> HasWallet(string walletNumber)
        {
            var response = new BasicResponse(false);

            var customer = await _customerDAO.FindByWalletNumber(walletNumber);
            response.IsSuccessful = customer != null && customer.IsActive;
            return response;
        }

        public async Task<BasicResponse> HasAcount(string accountNumber)
        {
            var response = new BasicResponse(false);

            var customer = await _customerDAO.FindByAccountNumber(accountNumber);
            response.IsSuccessful = customer != null && customer.IsActive == true;
            return response;
        }

        //Make sure the wallet number is equal to the persons phone number TELL THE FRONT END GUY
        public async Task<ServiceResponse<AccountOpeningResponse>> OpenAccountWithWallet(AccountOpeningRequestForCustomerWithWallet request, string language)
        {

            var response = new ServiceResponse<AccountOpeningResponse>(false);
            // var mapper = await FeedForOlderRecords(request);
            var customer = await _customerDAO.FindByWalletNumber(request.PhoneNumber);
            var wallet = await _requestDAO.Find(request.PhoneNumber);
            string selfieFileName = $"{request.PhoneNumber}{SELFIE_SURFIX}.{_settings.ImageFormat}";
            string signatureFileName = $"{request.PhoneNumber}{SIGNATURE_SURFIX}.{_settings.ImageFormat}";
            string IdFileName = $"{request.PhoneNumber}{ID_SURFIX}.{_settings.ImageFormat}";
            // var fileName = $"{request.PhoneNumber}.{_settings.ImageFormat}";

            var clientsSelfieLocation = await _imageManager.SaveImage(request.PhoneNumber, selfieFileName, DocumentType.PICTURE, request.Selfie.RawData);
            var clientsSignatureLocation = await _imageManager.SaveImage(request.PhoneNumber, signatureFileName, DocumentType.SIGNATURE, request.SignatureDto.RawData);
            var clientsIdImageLocation = await _imageManager.SaveImage(request.PhoneNumber, IdFileName, DocumentType.IDENTIFICATION, request.Identification.RawData);

            var biometricTag = Guid.NewGuid().ToString();
            var requestId = biometricTag;
            var refCode = await GetUniqueReferralCode();
            _logger.LogInformation("Finished generating the Referralcode in the Complete method of Walletopening service: referralcode:======>{0}", refCode);
            //Not imp;emented on the deployed App on production: Jsut new

            var refExist = await ReferralCodeExists(request.ReferralCode, language);
            var refBy = refExist.Exists == true ? request.ReferralCode : "";
            var accountRequest = new Service.Model.AccountOpeningRequest();

            if (customer != null)
            {

                accountRequest = new Service.Model.AccountOpeningRequest()
                {
                    PassportPhoto = clientsSelfieLocation,
                    ReferralCode = refCode,
                    RequestId = requestId
,
                    BiometricTag = biometricTag,
                    Signature = clientsSignatureLocation,
                    IdImage = clientsIdImageLocation,
                    AccountType = request.AccountType,
                    Address = wallet.Nationality,
                    BranchCode = request.BranchCode,
                    City = request.City,
                    Nationality = wallet.Nationality,
                    Country = wallet.Nationality,
                    CountryCode = _currencyCode.ConvertCountry(request.CountryCode),
                    DateCreated = DateTime.Now,
                    DateOfBirth = wallet.BirthDate,
                    Email = wallet.EmailAddress,
                    FirstName = wallet.FirstName,
                    Gender = wallet.Gender,
                    IdNumber = request.IdNumber,
                    IdType = request.IdType,
                    LastName = request.LastName,
                    MaritalStatus = request.MaritalStatus,
                    MiddleName = request.MiddleName,
                    ReferralBy = request.ReferralBy,
                    State = request.State,
                    Occupation = request.Occupation,
                    PostalCode = request.PostalCode,
                    Salutation = request.Salutation,
                    SelfieImage = request.Selfie.RawData,
                    WalletNumber = wallet.PhoneNumber,
                    StreetName = request.StreetName,
                    HouseNumber = request.HouseNumber,
                    AccountOpeningStatus = AccountOpeningStatus.SelfiePending,
                    AccountOpeningRecordStatus = AccountOpeningRecordStatus.INITIALISED
                };


                var accountPayload = AccountOpeningPayloadWithWallet(accountRequest);
                DocumentFeedOne(request, customer, selfieFileName, signatureFileName, IdFileName);
                DocumentFeedOne(request, customer, selfieFileName, signatureFileName, IdFileName);


                var device = await _deviceDAO.FindByCustomerId(customer.Id);

                if (request.PhoneNumber != wallet.PhoneNumber)
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.PHONE_NUMBER_MISMATCH,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.PHONE_NUMBER_MISMATCH, language)
                    };
                    return response;
                }

                if (device.Any(p => p.DeviceId == request.Device.DeviceId) == true)
                {
                    return await CreateAccountCustomerWithWallet(request, language, response, accountRequest);

                }//Second if statement
                else
                {
                    if (device.Count() >= DEVICE_COUNT)
                    {
                        var res = await AddDeviceIfAllowed(customer.Id, language);
                        response.Error = res.Error;
                        return response;

                    }//Third if statement
                    else
                    {
                        return await CreateAccountCustomerWithWallet(request, language, response, accountRequest);


                    }
                }


            }
            else
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE,
                    ResponseDescription
                       = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_OPENING_SIGNATURE_FAILURE, language),
                    AccountOpeningStatus = AccountOpeningStatus.SignaturePending
                };
                return response;
            }





        }

        public async Task<Customer> Getwallet(string walletNumber)
        {
            var wallet = await _customerDAO.FindByWalletNumber(walletNumber);
            return wallet;
        }

        public async Task<ServiceResponse<AccountOpeningResponse>> OpenAccountWithNoWallet(AccountOpeningCompositRequest request, String language)
        {
            var response = new ServiceResponse<AccountOpeningResponse>(false);
            var payload = AccountOpeningPayloadWithoutWallet(request);
            string selfieFileName = $"{request.AccountOpeningRequest.PhoneNumber}{SELFIE_SURFIX}.{_settings.ImageFormat}";
            string signatureFileName = $"{request.AccountOpeningRequest.PhoneNumber}{SIGNATURE_SURFIX}.{_settings.ImageFormat}";
            string IdFileName = $"{request.AccountOpeningRequest.PhoneNumber}{ID_SURFIX}.{_settings.ImageFormat}";
            // var fileName = $"{request.PhoneNumber}.{_settings.ImageFormat}";

            var clientsSelfieLocation = await _imageManager.SaveImage(request.AccountOpeningRequest.PhoneNumber, selfieFileName, DocumentType.PICTURE, request.Selfie.RawData);
            var clientsSignatureLocation = await _imageManager.SaveImage(request.AccountOpeningRequest.PhoneNumber, signatureFileName, DocumentType.SIGNATURE, request.DocumSignatureent.RawData);
            var clientsIdImageLocation = await _imageManager.SaveImage(request.AccountOpeningRequest.PhoneNumber, IdFileName, DocumentType.IDENTIFICATION, request.IdentificationDTO.RawData);

            var biometricTag = Guid.NewGuid().ToString();
            var requestId = biometricTag;
            var refCode = await GetUniqueReferralCode();
            var accountRequest = new Service.Model.AccountOpeningRequest()
            {
                PassportPhoto = clientsSelfieLocation,
                ReferralCode = refCode,
                RequestId = requestId
   ,
                BiometricTag = biometricTag,
                Signature = clientsSignatureLocation,
                IdImage = clientsIdImageLocation,
                AccountType = request.AccountOpeningRequest.AccountType,
                Address = request.AccountOpeningRequest.Nationality,
                BranchCode = request.AccountOpeningRequest.BranchCode,
                City = request.AccountOpeningRequest.City,
                Nationality = request.AccountOpeningRequest.Nationality,
                Country = request.AccountOpeningRequest.Nationality,
                CountryCode = _currencyCode.ConvertCountry(request.CountryCode),
                DateCreated = DateTime.Now,
                DateOfBirth = request.AccountOpeningRequest.DateOfBirth,
                Email = request.AccountOpeningRequest.Email,
                FirstName = request.AccountOpeningRequest.FirstName,
                Gender = request.AccountOpeningRequest.Gender,
                IdNumber = request.AccountOpeningRequest.IdNumber,
                IdType = request.AccountOpeningRequest.IdType,
                LastName = request.AccountOpeningRequest.LastName,
                MaritalStatus = request.AccountOpeningRequest.MaritalStatus,
                MiddleName = request.AccountOpeningRequest.MiddleName,
                ReferralBy = request.AccountOpeningRequest.ReferralBy,
                State = request.AccountOpeningRequest.State,
                Occupation = request.AccountOpeningRequest.Occupation,
                PostalCode = request.AccountOpeningRequest.PostalCode,
                Salutation = request.AccountOpeningRequest.Salutation,
                SelfieImage = request.AccountOpeningRequest.Selfie.RawData,
                WalletNumber = request.AccountOpeningRequest.PhoneNumber,
                StreetName = request.AccountOpeningRequest.StreetName,
                HouseNumber = request.AccountOpeningRequest.HouseNumber,
                AccountOpeningStatus = AccountOpeningStatus.SelfiePending,
                AccountOpeningRecordStatus = AccountOpeningRecordStatus.INITIALISED
            };


            var accountPayload = AccountOpeningPayloadWithWallet(accountRequest);

            var result = await CreateAccountCustomerWithNoWallet(request, language, response, accountRequest, clientsSelfieLocation, clientsSignatureLocation, clientsIdImageLocation);



            return response;
        }

        public async Task<ServiceResponse<AccountOpeningResponse>> AccountOnboarding(/*AccountOpeningOnboardingInitiationRequest*/ dynamic request, string language, bool hasWallet = false)
        {
            var mapper = await FeedForOlderRecords(request);
            var payload = new AccountOpeningPayload();
            var customer = await _customerDAO.FindByWalletNumber(request.PhoneNumber);
            var wallet = await _requestDAO.Find(request.PhoneNumber);
            if (customer != null)
            {
                var response = new ServiceResponse<AccountOpeningResponse>(false);

                if (hasWallet == true)
                {
                    payload = AccountOpeningPayloadWithWallet(mapper);
                }
                else
                {
                    payload = AccountOpeningPayloadWithoutWallet(request);
                }



                //Call FI Account Opening endpoint

                var result = new AccountOpeningResponse();

                result = await OpenAccountFI(payload);
                if (result.ResponseCode == SUCCESS_RESPONSE)
                {
                    var ticket = _customerDAO.Begin();

                    customer = await _customerDAO.Add(request.Customer);

                    if (customer != null)
                    {
                        _deviceDAO.Join(ticket);
                        _otpDAO.Join(ticket);
                        _documentDAO.Join(ticket);
                        _requestDAO.Join(ticket);
                        _customerDAO.Join(ticket);
                    }
                    else
                    {

                    }




                    //var refExist = await ReferralCodeExists(request.ReferralCode, language);

                    //var refBy = refExist.Exists == true ? request.ReferralCode : "";
                    //var customer = new Customer
                    //{
                    //    DateCreated = DateTime.UtcNow,
                    //    EmailAddress = item.EmailAddress,
                    //    FirstName = item.FirstName,
                    //    IsActive = true,
                    //    LastName = item.LastName,
                    //    MiddleName = item.MiddleName,
                    //    WalletNumber = item.PhoneNumber,
                    //    OnboardingStatus = OnboardingStatus.COMPLETED,
                    //    HasWallet = true,
                    //    Gender = item.Gender,
                    //    Title = item.Salutation,
                    //    ReferredBy = refBy,
                    //    ReferralCode = refCode
                    //};

                    //var document=new DocumentAccountOpening
                    //{
                    //     Comment=request.Document.Comment, CustomerId=result.c
                    //}




                }
                else
                {

                    response.Error = new ErrorResponse
                    {
                        ResponseCode = $"{ACCOUNT_OPENING_PREFIX}{result.ResponseCode}"
                    };
                    return response;
                }


                return response;

            }
            else
            {

            }




            throw new NotImplementedException();

        }
        #endregion
    }
}
