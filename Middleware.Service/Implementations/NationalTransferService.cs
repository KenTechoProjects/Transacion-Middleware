using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Middleware.Service.Model;
using Middleware.Service.DAO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Core.DTO;
using Middleware.Core.DAO;
using Middleware.Core.Model;
using Newtonsoft.Json;
using Middleware.Service.Extensions;
using Microsoft.Extensions.Configuration;

namespace Middleware.Service.Implementations
{
    public class NationalTransferService : INationalTransferService
    {
        private readonly IConfiguration _configuration;
        readonly ILocalAccountService _localService;
        readonly IMessageProvider _messageProvider;
        readonly IReversalDAO _reversalDAO;
        readonly IExternalTransferService _externalSrvice;
        readonly IInstitutionDAO _institutionDAO;
        readonly IAuthenticator _authenticator;
        readonly ILogger _logger;
        readonly SystemSettings _settings;
        readonly IBeneficiaryService _beneficiaryService;
        readonly ITransactionDAO _transactionDAO;
        readonly ILimitService _limitService;
        private readonly IWalletService _walletService;
        private const string WALLET_SUCCESS_CODE = "00";
        private const string TRANSFER_SUCCESS_CODE = "00";
        private readonly IInterWalletService _interWalletService;
        private const string DEBIT_ERR_PREFIX = "DBF";
        private const string CREDIT_ERR_PREFIX = "CDF";
        private const string CEVA_NOTATION_PREFIX = "CEVA";
        private const string FI_NOTATION_PREFIX = "FI";
        private const string FI_ERROR = "";
        private const string NRFI_ERROR = "NRFI";
        private const string INTERWALLET_NOTATION_PREFIX = "IW";
        private readonly Decimal MaxSingleTransactionWithPin = 0;
        private readonly Decimal MaxDailyTransactionWithPin = 0;
        private readonly Decimal MaxSingleTransactionWithToken = 0;
        private readonly Decimal MaxDailyTransactionWithToken = 0;
        public NationalTransferService(ILocalAccountService localService, IExternalTransferService externalSrvice,
                                        IInstitutionDAO institutionDAO, IMessageProvider messageProvider, IReversalDAO reversalDAO,
                                        IAuthenticator authenticator, ILoggerFactory logger, IOptions<SystemSettings> settingsProvider,
                                        IBeneficiaryService beneficiaryService, ITransactionDAO transactionDAO, ILimitService limitService,
                                        IWalletService walletService, IInterWalletService interWalletService, IConfiguration configuration)
        {
            _localService = localService;
            _externalSrvice = externalSrvice;
            _institutionDAO = institutionDAO;
            _messageProvider = messageProvider;
            _reversalDAO = reversalDAO;
            _authenticator = authenticator;
            _settings = settingsProvider.Value;
            _logger = logger.CreateLogger(typeof(NationalTransferService));
            _beneficiaryService = beneficiaryService;
            _transactionDAO = transactionDAO;
            _limitService = limitService;
            _walletService = walletService;
            _interWalletService = interWalletService;
            _configuration = configuration;
            MaxSingleTransactionWithPin = _configuration.GetValue<decimal>("TwoFactorAuthenticationTransactionLimits:MaxSingleTransactionWithPin");
            MaxDailyTransactionWithPin = _configuration.GetValue<decimal>("TwoFactorAuthenticationTransactionLimits:MaxDailyTransactionWithPin");
            MaxSingleTransactionWithToken = _configuration.GetValue<decimal>("TwoFactorAuthenticationTransactionLimits:MaxSingleTransactionWithToken");
            MaxDailyTransactionWithToken = _configuration.GetValue<decimal>("TwoFactorAuthenticationTransactionLimits:MaxDailyTransactionWithToken");
        }

        public async Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber, string institutionCode, string language)
        {
            _logger.LogInformation("Inside GetAccountName method of NationalTransferService ");

            ServiceResponse<dynamic> response = new ServiceResponse<dynamic>(false);
            if (_settings.BankCode.Equals(institutionCode, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogInformation("Request passed to _localService.GetAccountName Method {0}", new { accountNumber, institutionCode, language });


                //response = await _localService.GetAccountName(accountNumber);
                response = await _localService.GetAccountName(accountNumber, institutionCode);
                _logger.LogInformation("Response gotten from  _localService.GetAccountName inside GetAccountName Method of LocalAccountService {0}", JsonConvert.SerializeObject(new { accountNumber, institutionCode, language }));
            }
            else
            {
                if (_externalSrvice != null)
                {
                    _logger.LogInformation("Request passed to _externalSrvice.GetAccountName Method {0}", new { accountNumber, institutionCode, language });


                    response = await _externalSrvice.GetAccountName(accountNumber, institutionCode);
                    _logger.LogInformation("Response gotten from  _externalSrvice.GetAccountName inside GetAccountName Method of ExternalTransferService {0}", JsonConvert.SerializeObject(new { accountNumber, institutionCode, language }));

                }
                else
                {
                    _logger.LogInformation("_externalSrvice can not be null");

                }

            }

            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);

                return response;
            }


            return response;
        }
        #region 2AF new
        public async Task<ServiceResponseT<TransferResponse>> Transfer2FA(AuthenticatedTransferRequest request, AuthenticatedUser user, string language, bool saveAsBeneficiary)
        {
            if (string.IsNullOrWhiteSpace(request.Narration))
            {
                request.Narration = null;
            }
            _logger.LogInformation("Inside Transfer  method of NationalTransferService ");
            var response = new ServiceResponseT<TransferResponse>(false);
            var isLocal = _settings.BankCode.Equals(request.DestinationInstitutionId, StringComparison.InvariantCultureIgnoreCase);
            if (!request.IsValid(out var source))
            {
                response = new ServiceResponseT<TransferResponse>(false)
                {
                    FaultType = FaultMode.CLIENT_INVALID_ARGUMENT,
                    Error = new ErrorResponseT
                    {
                        ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}"
                    }
                };
                return response;
            }

            if (!isLocal && request.DestinationAccountId.Length < _settings.AccountNumberLength)
            {
                return ErrorResponseT.Create<ServiceResponseT<TransferResponse>>(FaultMode.CLIENT_INVALID_ARGUMENT,
                    ResponseCodes.INVALID_ACCOUNT_LENGTH, _messageProvider.GetMessage(ResponseCodes.INVALID_ACCOUNT_LENGTH, language));

            }

            var validationResponse = await _authenticator.ValidatePin(user.UserName, request.Pin);
            _logger.LogInformation("Response gotten from Transfer Method of NationalTransferService {0}", validationResponse);
            if (!validationResponse.IsSuccessful)
            {
                response = new ServiceResponseT<TransferResponse>(false)
                {
                    FaultType = FaultMode.UNAUTHORIZED,
                    Error = new ErrorResponseT
                    {
                        ResponseCode = validationResponse.Error?.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(validationResponse.Error?.ResponseCode, language)
                    }
                };
                return response;
            }

            //MaxSingleTransactionWithPin
            // MaxDailyTransactionWithPin



            var transactionReference = Guid.NewGuid().ToString();
            BasicResponse paymentResponse = new BasicResponse();
            var isWithinLimit = new TransactionLimitResponse();
            if (request.IsLimitExceeded == false)
            {
                isWithinLimit = await _limitService.ValidateLimit(user.Id, TransactionType.NationalTransfer, request.Amount);
                if (isWithinLimit.NotFound == true)
                {
                    _logger.LogInformation("transaction limit has not been set it returns null ");
                    response.Error = new ErrorResponseT()
                    {
                        ResponseCode = ResponseCodes.TRANSACTION_LIMIT_NOT_YET_SET,
                        ResponseDescription =
                            _messageProvider.GetMessage(ResponseCodes.TRANSACTION_LIMIT_NOT_YET_SET, language)
                    };
                    return response;

                }
                else if (isWithinLimit.IsLimitExceeded == true)

                {


                    if (request.Amount >= MaxSingleTransactionWithPin)
                    {
                        _logger.LogInformation("transaction limit errorCode {errorode}", ResponseCodes.LIMIT_EXCEED_FOR_OTP);
                        response = new ServiceResponseT<TransferResponse>(false)
                        {
                            FaultType = FaultMode.LIMIT_EXCEED_FOR_OTP,
                            Error = new ErrorResponseT
                            {
                                ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_OTP,
                                IsLimitExceeded = true,
                                IsTransactionNeedOTP = true,
                                ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_OTP, language)}"
                            }

                        };
                        return response;
                    }
                    else if (request.Amount >= MaxDailyTransactionWithPin)

                    {
                        _logger.LogInformation("transaction limit errorCode {errorode}", ResponseCodes.LIMIT_EXCEED_FOR_OTP);
                        response = new ServiceResponseT<TransferResponse>(false)
                        {
                            FaultType = FaultMode.LIMIT_EXCEED_FOR_OTP,
                            Error = new ErrorResponseT
                            {
                                ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_OTP,
                                IsLimitExceeded = true,
                                IsTransactionNeedOTP = true,
                                ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_OTP, language)}"
                            }

                        };
                        return response;
                    }
                    else if (request.Amount >= MaxSingleTransactionWithToken)

                    {
                        response = new ServiceResponseT<TransferResponse>(false)
                        {
                            FaultType = FaultMode.LIMIT_EXCEED_FOR_TOKEN,
                            Error = new ErrorResponseT
                            {
                                ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_TOKEN,
                                IsLimitExceeded = true,
                                IsTransactionNeedOTP = true,
                                ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_TOKEN, language)}"
                            }

                        };
                        return response;
                    }
                    else if (request.Amount >= MaxDailyTransactionWithToken)

                    {
                        response = new ServiceResponseT<TransferResponse>(false)
                        {
                            FaultType = FaultMode.LIMIT_EXCEED_FOR_TOKEN,
                            Error = new ErrorResponseT
                            {
                                ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_TOKEN,
                                IsLimitExceeded = true,
                                IsTransactionNeedOTP = true,
                                ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_TOKEN, language)}"
                            }

                        };
                        return response;
                    }
                    else if (request.Amount < MaxSingleTransactionWithPin && request.Amount < MaxDailyTransactionWithPin && request.Amount < MaxSingleTransactionWithToken && request.Amount < MaxDailyTransactionWithToken)

                    {
                        response = new ServiceResponseT<TransferResponse>(false)
                        {
                            FaultType = FaultMode.LIMIT_EXCEEDED,
                            Error = new ErrorResponseT
                            {
                                ResponseCode = ResponseCodes.LIMIT_EXCEEDED,
                                IsLimitExceeded = true,
                                IsTransactionNeedOTP = false,
                                ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEEDED, language)}"
                            }

                        };
                        return response;
                    }


                }
                else //if (isWithinLimit.IsLimitExceeded == false)
                {
                    if (request.Amount >= MaxSingleTransactionWithPin)
                    {

                        response = new ServiceResponseT<TransferResponse>(false)
                        {
                            FaultType = FaultMode.LIMIT_EXCEED_FOR_OTP,
                            Error = new ErrorResponseT
                            {
                                ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_OTP,
                                IsLimitExceeded = false,
                                IsTransactionNeedOTP = true,
                                ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_OTP, language)}"
                            }

                        };
                        return response;
                    }
                    else if (request.Amount >= MaxDailyTransactionWithPin)
                    {

                        response = new ServiceResponseT<TransferResponse>(false)
                        {
                            FaultType = FaultMode.LIMIT_EXCEED_FOR_OTP,
                            Error = new ErrorResponseT
                            {
                                ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_OTP,
                                IsLimitExceeded = false,
                                IsTransactionNeedOTP = true,
                                ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_OTP, language)}"
                            }

                        };
                        return response;
                    }

                    else if (request.Amount >= MaxSingleTransactionWithToken)
                    {

                        response = new ServiceResponseT<TransferResponse>(false)
                        {
                            FaultType = FaultMode.LIMIT_EXCEED_FOR_TOKEN,
                            Error = new ErrorResponseT
                            {
                                ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_TOKEN,
                                IsLimitExceeded = false,
                                IsTransactionNeedOTP = true,
                                ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_TOKEN, language)}"
                            }

                        };
                        return response;
                    }
                    else if (request.Amount >= MaxDailyTransactionWithToken)
                    {

                        response = new ServiceResponseT<TransferResponse>(false)
                        {
                            FaultType = FaultMode.LIMIT_EXCEED_FOR_TOKEN,
                            Error = new ErrorResponseT
                            {
                                ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_TOKEN,
                                IsLimitExceeded = false,
                                IsTransactionNeedOTP = true,
                                ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_TOKEN, language)}"
                            }

                        };
                        return response;
                    }

                    else
                    {

                        if (!string.IsNullOrEmpty(request.Narration))
                        {
                            request.Narration = Util.EncodeString($"{_settings.NarrationPrefix}{request.Narration}");
                        }

                        var transaction = new Transaction
                        {
                            Amount = request.Amount,
                            CustomerId = user.Id,
                            DateCreated = DateTime.Now,
                            Narration = request.Narration,
                            TransactionReference = transactionReference,
                            SourceAccountId = request.SourceAccountId,
                            TransactionType = TransactionType.NationalTransfer,
                            DestinationAccountID = request.DestinationAccountId,
                            DestinationInstitution = request.DestinationInstitutionId,
                            TransactionStatus = TransactionStatus.New,
                            //Added to trac for customer's transaction notification
                            DestinationTransactionTag = TransactionTag.New,
                            SourceTransactionTag = TransactionTag.New,
                            ResponseCode = FI_ERROR //This line is logged because FI lost in transit without returning response
                        };

                        transaction.ID = await _transactionDAO.Add(transaction);
                        _logger.LogInformation("Response gotten from Transfer Method of NationaltransferService  on a call to TransactionDAO : {0}", JsonConvert.SerializeObject(transaction.ID));
                        try
                        {
                            if (isLocal == true)
                            {
                                paymentResponse = await _localService.Transfer(request, transactionReference);

                                _logger.LogInformation("Response gotten from Transfer Method of NationaltransferService  on a call to LocalAccountService : {0}", JsonConvert.SerializeObject(transaction.ID));
                            }
                            else
                            {
                                paymentResponse = await _externalSrvice.Transfer(request, transactionReference);
                                _logger.LogInformation("Response gotten from Transfer Method of NationaltransferService  on a call to ExternalAccountService : {0}", JsonConvert.SerializeObject(transaction.ID));
                            }
                        }
                        catch (Exception ex)
                        {

                            _logger.LogCritical(ex, "EXCEPTION WHILE CALLING FI");
                            transaction.TransactionStatus = TransactionStatus.FIException;
                            transaction.ResponseCode = paymentResponse.Error?.ResponseCode;
                            if (string.IsNullOrEmpty(transaction.ResponseCode))
                            {
                                transaction.ResponseCode = NRFI_ERROR;
                            }
                            transaction.ResponseTime = DateTime.Now;
                            await _transactionDAO.Update(transaction);

                            response.FaultType = FaultMode.GATEWAY_ERROR;
                            response.Error = new ErrorResponseT()
                            {
                                ResponseCode = ResponseCodes.GENERAL_ERROR,
                                ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                            };
                            _logger.LogCritical(ex, "Response gotten from Transfer Method of NationaltransferService at {0}", DateTime.UtcNow);
                            if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
                            {
                                response.Error = new ErrorResponseT()
                                {
                                    ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                                    ResponseDescription =
                                        _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                                };
                                return response;
                            }
                            return response;
                        }


                        transaction.ResponseTime = DateTime.Now;
                        transaction.ResponseCode = paymentResponse.IsSuccessful ? TRANSFER_SUCCESS_CODE : paymentResponse.Error.ResponseCode;
                        if (string.IsNullOrEmpty(transaction.ResponseCode))
                        {
                            transaction.ResponseCode = NRFI_ERROR;
                        }
                        transaction.TransactionStatus = paymentResponse.IsSuccessful ? TransactionStatus.Successful : TransactionStatus.Failed;

                        await _transactionDAO.Update(transaction);

                        if (!paymentResponse.IsSuccessful)
                        {
                            response.Error = new ErrorResponseT()
                            {
                                ResponseCode = paymentResponse.Error.ResponseCode,
                                ResponseDescription = _messageProvider.GetMessage(paymentResponse.Error.ResponseCode, language)
                            };

                            return response;
                        }
                        response.IsSuccessful = true;

                        var transferResponse = new TransferResponse
                        {
                            Date = DateTime.Now,
                            Reference = transactionReference,
                            BeneficiaryStatus = new BeneficiaryStatus
                            {
                                Attempted = saveAsBeneficiary,
                            },
                            TransactionDetails = new TransactionDetails
                            {
                                Amount = request.Amount,
                                DestinationAccountName = request.DestinationAccountName,
                                DestinationAccountID = request.DestinationAccountId,
                                Narration = Util.DecodeString(request.Narration),
                                SourceAccountName = request.SourceAccountName,
                                SourceAccountNumber = request.SourceAccountId,
                                DestinationBank = request.DestinationInstitutionId
                            }
                        };
                        _logger.LogInformation("saveAsBeneficiary {0}", saveAsBeneficiary);
                        if (saveAsBeneficiary == true)
                        {
                            if (!string.IsNullOrEmpty(request.DestinationAccountName))
                            {
                                request.DestinationAccountName = Util.EncodeString(request.DestinationAccountName);
                            }
                            var beneficaryStatus = await SaveBeneficiary(request, user.UserName, AccountType.BANK);

                            transferResponse.BeneficiaryStatus.IsSuccessful = beneficaryStatus.IsSuccessful;
                            var beneiciaryResponseCode = beneficaryStatus.IsSuccessful ? ResponseCodes.BENEFICIARY_SAVED_SUCCESSFULLY : ResponseCodes.UNABLE_TO_SAVE_BENEFICIARY;
                            transferResponse.BeneficiaryStatus.Message = _messageProvider.GetMessage(beneiciaryResponseCode, language);
                            _logger.LogInformation(" Response from saveAsBeneficiary {0}", JsonConvert.SerializeObject(transferResponse));
                        }

                        response.SetPayload(transferResponse);
                        return response;
                    }


                }

            }
            else
            {
                if (!string.IsNullOrEmpty(request.Narration))
                {
                    request.Narration = Util.EncodeString($"{_settings.NarrationPrefix}{request.Narration}");
                }

                var transaction = new Transaction
                {
                    Amount = request.Amount,
                    CustomerId = user.Id,
                    DateCreated = DateTime.Now,
                    Narration = request.Narration,
                    TransactionReference = transactionReference,
                    SourceAccountId = request.SourceAccountId,
                    TransactionType = TransactionType.NationalTransfer,
                    DestinationAccountID = request.DestinationAccountId,
                    DestinationInstitution = request.DestinationInstitutionId,
                    TransactionStatus = TransactionStatus.New,
                    ResponseCode = FI_ERROR //This line is logged because FI lost in transit without returning response
                };

                transaction.ID = await _transactionDAO.Add(transaction);
                _logger.LogInformation("Response gotten from Transfer Method of NationaltransferService  on a call to TransactionDAO : {0}", JsonConvert.SerializeObject(transaction.ID));
                try
                {
                    if (isLocal == true)
                    {
                        paymentResponse = await _localService.Transfer(request, transactionReference);

                        _logger.LogInformation("Response gotten from Transfer Method of NationaltransferService  on a call to LocalAccountService : {0}", JsonConvert.SerializeObject(transaction.ID));
                    }
                    else
                    {
                        paymentResponse = await _externalSrvice.Transfer(request, transactionReference);
                        _logger.LogInformation("Response gotten from Transfer Method of NationaltransferService  on a call to ExternalAccountService : {0}", JsonConvert.SerializeObject(transaction.ID));
                    }
                }
                catch (Exception ex)
                {

                    _logger.LogCritical(ex, "EXCEPTION WHILE CALLING FI");
                    transaction.TransactionStatus = TransactionStatus.FIException;
                    transaction.ResponseCode = paymentResponse.Error?.ResponseCode;
                    if (string.IsNullOrEmpty(transaction.ResponseCode))
                    {
                        transaction.ResponseCode = NRFI_ERROR;
                    }
                    transaction.ResponseTime = DateTime.Now;
                    await _transactionDAO.Update(transaction);

                    response.FaultType = FaultMode.GATEWAY_ERROR;
                    response.Error = new ErrorResponseT()
                    {
                        ResponseCode = ResponseCodes.GENERAL_ERROR,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                    };
                    _logger.LogCritical(ex, "Response gotten from Transfer Method of NationaltransferService at {0}", DateTime.UtcNow);
                    if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
                    {
                        response.Error = new ErrorResponseT()
                        {
                            ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                            ResponseDescription =
                                _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                        };
                        return response;
                    }
                    return response;
                }
                transaction.ResponseTime = DateTime.Now;
                transaction.ResponseCode = paymentResponse.IsSuccessful ? TRANSFER_SUCCESS_CODE : paymentResponse.Error.ResponseCode;
                if (string.IsNullOrEmpty(transaction.ResponseCode))
                {
                    transaction.ResponseCode = NRFI_ERROR;
                }
                transaction.TransactionStatus = paymentResponse.IsSuccessful ? TransactionStatus.Successful : TransactionStatus.Failed;

                await _transactionDAO.Update(transaction);

                if (!paymentResponse.IsSuccessful)
                {
                    response.Error = new ErrorResponseT()
                    {
                        ResponseCode = paymentResponse.Error.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(paymentResponse.Error.ResponseCode, language)
                    };

                    return response;
                }
                response.IsSuccessful = true;

                var transferResponse = new TransferResponse
                {
                    Date = DateTime.Now,
                    Reference = transactionReference,
                    BeneficiaryStatus = new BeneficiaryStatus
                    {
                        Attempted = saveAsBeneficiary,
                    },
                    TransactionDetails = new TransactionDetails
                    {
                        Amount = request.Amount,
                        DestinationAccountName = request.DestinationAccountName,
                        DestinationAccountID = request.DestinationAccountId,
                        Narration = Util.DecodeString(request.Narration),
                        SourceAccountName = request.SourceAccountName,
                        SourceAccountNumber = request.SourceAccountId,
                        DestinationBank = request.DestinationInstitutionId
                    }
                };
                _logger.LogInformation("saveAsBeneficiary {0}", saveAsBeneficiary);
                if (saveAsBeneficiary == true)
                {
                    if (!string.IsNullOrEmpty(request.DestinationAccountName))
                    {
                        request.DestinationAccountName = Util.EncodeString(request.DestinationAccountName);
                    }
                    var beneficaryStatus = await SaveBeneficiary(request, user.UserName, AccountType.BANK);

                    transferResponse.BeneficiaryStatus.IsSuccessful = beneficaryStatus.IsSuccessful;
                    var beneiciaryResponseCode = beneficaryStatus.IsSuccessful ? ResponseCodes.BENEFICIARY_SAVED_SUCCESSFULLY : ResponseCodes.UNABLE_TO_SAVE_BENEFICIARY;
                    transferResponse.BeneficiaryStatus.Message = _messageProvider.GetMessage(beneiciaryResponseCode, language);
                    _logger.LogInformation(" Response from saveAsBeneficiary {0}", JsonConvert.SerializeObject(transferResponse));
                }

                response.SetPayload(transferResponse);
                return response;
            }

            return response;


        }


        #endregion

        #region Old Transaction Limit Check
        #region Old Transaction
        public async Task<ServiceResponseT<TransferResponse>> Transfer(AuthenticatedTransferRequest request, AuthenticatedUser user, string language, bool saveAsBeneficiary)
        {
            if (string.IsNullOrWhiteSpace(request.Narration))
            {
                request.Narration = null;
            }
            _logger.LogInformation("Inside Transfer  method of NationalTransferService ");
            var response = new ServiceResponseT<TransferResponse>(false);
            var isLocal = _settings.BankCode.Equals(request.DestinationInstitutionId, StringComparison.InvariantCultureIgnoreCase);
            if (!request.IsValid(out var source))
            {
                response = new ServiceResponseT<TransferResponse>(false)
                {
                    FaultType = FaultMode.CLIENT_INVALID_ARGUMENT,
                    Error = new ErrorResponseT
                    {
                        ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}"
                    }
                };
                return response;
            }

            if (!isLocal && request.DestinationAccountId.Length < _settings.AccountNumberLength)
            {
                return ErrorResponseT.Create<ServiceResponseT<TransferResponse>>(FaultMode.CLIENT_INVALID_ARGUMENT,
                    ResponseCodes.INVALID_ACCOUNT_LENGTH, _messageProvider.GetMessage(ResponseCodes.INVALID_ACCOUNT_LENGTH, language));

            }

            var validationResponse = await _authenticator.ValidatePin(user.UserName, request.Pin);
            _logger.LogInformation("Response gotten from Transfer Method of NationalTransferService {0}", validationResponse);
            if (!validationResponse.IsSuccessful)
            {
                response = new ServiceResponseT<TransferResponse>(false)
                {
                    FaultType = FaultMode.UNAUTHORIZED,
                    Error = new ErrorResponseT
                    {
                        ResponseCode = validationResponse.Error?.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(validationResponse.Error?.ResponseCode, language)
                    }
                };
                return response;
            }
            var transactionReference = Guid.NewGuid().ToString();
            BasicResponse paymentResponse = new BasicResponse();

            var isWithinLimit = await _limitService.ValidateLimit(user.Id, TransactionType.NationalTransfer, request.Amount);
            if (isWithinLimit.NotFound == true)
            {
                _logger.LogInformation("transaction limit has not been set it returns null ");
                response.Error = new ErrorResponseT()
                {
                    ResponseCode = ResponseCodes.TRANSACTION_LIMIT_NOT_YET_SET,
                    ResponseDescription =
                        _messageProvider.GetMessage(ResponseCodes.TRANSACTION_LIMIT_NOT_YET_SET, language)
                };
                return response;

            }
            if (isWithinLimit.IsLimitExceeded == true)
            {
                response = new ServiceResponseT<TransferResponse>(false)
                {
                    FaultType = FaultMode.LIMIT_EXCEEDED,
                    Error = new ErrorResponseT
                    {
                        ResponseCode = ResponseCodes.LIMIT_EXCEEDED,


                        ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEEDED, language)}"
                    }

                };
                return response;
            }
            if (!string.IsNullOrEmpty(request.Narration))
            {
                request.Narration = Util.EncodeString($"{_settings.NarrationPrefix}{request.Narration}");
            }

            var transaction = new Transaction
            {
                Amount = request.Amount,
                CustomerId = user.Id,
                DateCreated = DateTime.Now,
                Narration = request.Narration,
                TransactionReference = transactionReference,
                SourceAccountId = request.SourceAccountId,
                TransactionType = TransactionType.NationalTransfer,
                DestinationAccountID = request.DestinationAccountId,
                DestinationInstitution = request.DestinationInstitutionId,
                TransactionStatus = TransactionStatus.New,
                ResponseCode = FI_ERROR //This line is logged because FI lost in transit without returning response
            };

            transaction.ID = await _transactionDAO.Add(transaction);
            _logger.LogInformation("Response gotten from Transfer Method of NationaltransferService  on a call to TransactionDAO : {0}", JsonConvert.SerializeObject(transaction.ID));
            try
            {
                if (isLocal == true)
                {
                    paymentResponse = await _localService.Transfer(request, transactionReference);

                    _logger.LogInformation("Response gotten from Transfer Method of NationaltransferService  on a call to LocalAccountService : {0}", JsonConvert.SerializeObject(transaction.ID));
                }
                else
                {
                    paymentResponse = await _externalSrvice.Transfer(request, transactionReference);
                    _logger.LogInformation("Response gotten from Transfer Method of NationaltransferService  on a call to ExternalAccountService : {0}", JsonConvert.SerializeObject(transaction.ID));
                }
            }
            catch (Exception ex)
            {

                _logger.LogCritical(ex, "EXCEPTION WHILE CALLING FI");
                transaction.TransactionStatus = TransactionStatus.FIException;
                transaction.ResponseCode = paymentResponse.Error?.ResponseCode;
                if (string.IsNullOrEmpty(transaction.ResponseCode))
                {
                    transaction.ResponseCode = NRFI_ERROR;
                }
                transaction.ResponseTime = DateTime.Now;
                await _transactionDAO.Update(transaction);

                response.FaultType = FaultMode.GATEWAY_ERROR;
                response.Error = new ErrorResponseT()
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };
                _logger.LogCritical(ex, "Response gotten from Transfer Method of NationaltransferService at {0}", DateTime.UtcNow);
                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
                {
                    response.Error = new ErrorResponseT()
                    {
                        ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                        ResponseDescription =
                            _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                    };
                    return response;
                }
                return response;
            }
            transaction.ResponseTime = DateTime.Now;
            transaction.ResponseCode = paymentResponse.IsSuccessful ? TRANSFER_SUCCESS_CODE : paymentResponse.Error.ResponseCode;
            if (string.IsNullOrEmpty(transaction.ResponseCode))
            {
                transaction.ResponseCode = NRFI_ERROR;
            }
            transaction.TransactionStatus = paymentResponse.IsSuccessful ? TransactionStatus.Successful : TransactionStatus.Failed;

            await _transactionDAO.Update(transaction);

            if (!paymentResponse.IsSuccessful)
            {
                response.Error = new ErrorResponseT()
                {
                    ResponseCode = paymentResponse.Error.ResponseCode,
                    ResponseDescription = _messageProvider.GetMessage(paymentResponse.Error.ResponseCode, language)
                };

                return response;
            }
            response.IsSuccessful = true;

            var transferResponse = new TransferResponse
            {
                Date = DateTime.Now,
                Reference = transactionReference,
                BeneficiaryStatus = new BeneficiaryStatus
                {
                    Attempted = saveAsBeneficiary,
                },
                TransactionDetails = new TransactionDetails
                {
                    Amount = request.Amount,
                    DestinationAccountName = request.DestinationAccountName,
                    DestinationAccountID = request.DestinationAccountId,
                    Narration = Util.DecodeString(request.Narration),
                    SourceAccountName = request.SourceAccountName,
                    SourceAccountNumber = request.SourceAccountId,
                    DestinationBank = request.DestinationInstitutionId
                }
            };
            _logger.LogInformation("saveAsBeneficiary {0}", saveAsBeneficiary);
            if (saveAsBeneficiary == true)
            {
                if (!string.IsNullOrEmpty(request.DestinationAccountName))
                {
                    request.DestinationAccountName = Util.EncodeString(request.DestinationAccountName);
                }
                var beneficaryStatus = await SaveBeneficiary(request, user.UserName, AccountType.BANK);

                transferResponse.BeneficiaryStatus.IsSuccessful = beneficaryStatus.IsSuccessful;
                var beneiciaryResponseCode = beneficaryStatus.IsSuccessful ? ResponseCodes.BENEFICIARY_SAVED_SUCCESSFULLY : ResponseCodes.UNABLE_TO_SAVE_BENEFICIARY;
                transferResponse.BeneficiaryStatus.Message = _messageProvider.GetMessage(beneiciaryResponseCode, language);
                _logger.LogInformation(" Response from saveAsBeneficiary {0}", JsonConvert.SerializeObject(transferResponse));
            }

            response.SetPayload(transferResponse);
            return response;
        }

        #endregion

        #endregion
        public async Task<ServiceResponseT<TransferResponse>> AccountToWallet(AuthenticatedTransferRequest request, AuthenticatedUser user, string language, bool saveAsBeneficiary)
        {
            _logger.LogInformation("Inside  AccountToWallet  method of NationalTransferService ");
            if (string.IsNullOrWhiteSpace(request.Narration))
            {
                request.Narration = null;
            }
            var response = new ServiceResponseT<TransferResponse>(false);
            try
            {
                if (request.IsValid(out var source) == false)
                {
                    response = new ServiceResponseT<TransferResponse>(false)
                    {
                        FaultType = FaultMode.CLIENT_INVALID_ARGUMENT,
                        Error = new ErrorResponseT
                        {
                            ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                            ResponseDescription =
                                $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}"
                        }
                    };
                    _logger.LogInformation("Invalid input parameter  AccountToWallet  method of NationalTransferService . INPUT:===>{0}", JsonConvert.SerializeObject(new { request, user, language, saveAsBeneficiary }));
                    return response;
                }

                var validationResponse = await _authenticator.ValidatePin(user.UserName, request.Pin);
                _logger.LogInformation(
                    "Response from _authenticator.ValidatePin in the NationalTranserService. Response:====> {0}",
                    JsonConvert.SerializeObject(validationResponse));
                if (validationResponse.IsSuccessful == false)
                {
                    response = new ServiceResponseT<TransferResponse>(false)
                    {
                        FaultType = FaultMode.UNAUTHORIZED,
                        Error = new ErrorResponseT
                        {
                            ResponseCode = validationResponse.Error?.ResponseCode,
                            ResponseDescription =
                                _messageProvider.GetMessage(validationResponse.Error?.ResponseCode, language)
                        }
                    };
                    _logger.LogInformation("Unauthorized inside AccountToWallet  method of NationalTransferService");

                    return response;
                }

                var transactionReference = DateTime.Now.Ticks.ToString();

                var isWithinLimit =
                    await _limitService.ValidateLimit(user.Id, TransactionType.NationalTransfer, request.Amount);

                if (request.IsLimitExceeded == false)
                {
                    if (isWithinLimit.NotFound == true)
                    {
                        _logger.LogInformation("transaction limit has not been set it returns null ");
                        response.Error = new ErrorResponseT()
                        {
                            ResponseCode = ResponseCodes.TRANSACTION_LIMIT_NOT_YET_SET,
                            ResponseDescription =
                                _messageProvider.GetMessage(ResponseCodes.TRANSACTION_LIMIT_NOT_YET_SET, language)
                        };
                        return response;

                    }

                    //if (isWithinLimit.IsLimitExceeded == true)
                    //{
                    //    response = new ServiceResponse<TransferResponse>(false)
                    //    {
                    //        FaultType = FaultMode.INVALID_OBJECT_STATE,
                    //        Error = new ErrorResponse
                    //        {
                    //            ResponseCode = ResponseCodes.LIMIT_EXCEEDED,
                    //            ResponseDescription =
                    //                $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEEDED, language)}"
                    //        }

                    //    };
                    //    return response;
                    //}
                    else if (isWithinLimit.IsLimitExceeded == true)

                    {


                        if (request.Amount >= MaxSingleTransactionWithPin)
                        {
                            response = new ServiceResponseT<TransferResponse>(false)
                            {
                                FaultType = FaultMode.LIMIT_EXCEED_FOR_OTP,
                                Error = new ErrorResponseT
                                {
                                    ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_OTP,
                                    IsLimitExceeded = true,
                                    IsTransactionNeedOTP = true,
                                    ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_OTP, language)}"
                                }

                            };
                            return response;
                        }
                        else if (request.Amount >= MaxDailyTransactionWithPin)

                        {
                            response = new ServiceResponseT<TransferResponse>(false)
                            {
                                FaultType = FaultMode.LIMIT_EXCEED_FOR_OTP,
                                Error = new ErrorResponseT
                                {
                                    ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_OTP,
                                    IsLimitExceeded = true,
                                    IsTransactionNeedOTP = true,
                                    ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_OTP, language)}"
                                }

                            };
                            return response;
                        }
                        else if (request.Amount >= MaxSingleTransactionWithToken)

                        {
                            response = new ServiceResponseT<TransferResponse>(false)
                            {
                                FaultType = FaultMode.LIMIT_EXCEED_FOR_TOKEN,
                                Error = new ErrorResponseT
                                {
                                    ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_TOKEN,
                                    IsLimitExceeded = true,
                                    IsTransactionNeedOTP = true,
                                    ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_TOKEN, language)}"
                                }

                            };
                            return response;
                        }
                        else if (request.Amount >= MaxDailyTransactionWithToken)

                        {
                            response = new ServiceResponseT<TransferResponse>(false)
                            {
                                FaultType = FaultMode.LIMIT_EXCEED_FOR_TOKEN,
                                Error = new ErrorResponseT
                                {
                                    ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_TOKEN,
                                    IsLimitExceeded = true,
                                    IsTransactionNeedOTP = true,
                                    ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_TOKEN, language)}"
                                }

                            };
                            return response;
                        }
                        else if (request.Amount < MaxSingleTransactionWithPin && request.Amount < MaxDailyTransactionWithPin && request.Amount < MaxSingleTransactionWithToken && request.Amount < MaxDailyTransactionWithToken)

                        {
                            response = new ServiceResponseT<TransferResponse>(false)
                            {
                                FaultType = FaultMode.LIMIT_EXCEEDED,
                                Error = new ErrorResponseT
                                {
                                    ResponseCode = ResponseCodes.LIMIT_EXCEEDED,
                                    IsLimitExceeded = true,
                                    IsTransactionNeedOTP = false,
                                    ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEEDED, language)}"
                                }

                            };
                            return response;
                        }


                    }
                    else //if (isWithinLimit.IsLimitExceeded == false)
                    {
                        if (request.Amount >= MaxSingleTransactionWithPin)
                        {

                            response = new ServiceResponseT<TransferResponse>(false)
                            {
                                FaultType = FaultMode.LIMIT_EXCEED_FOR_OTP,
                                Error = new ErrorResponseT
                                {
                                    ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_OTP,
                                    IsLimitExceeded = false,
                                    IsTransactionNeedOTP = true,
                                    ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_OTP, language)}"
                                }

                            };
                            return response;
                        }
                        else if (request.Amount >= MaxDailyTransactionWithPin)
                        {

                            response = new ServiceResponseT<TransferResponse>(false)
                            {
                                FaultType = FaultMode.LIMIT_EXCEED_FOR_OTP,
                                Error = new ErrorResponseT
                                {
                                    ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_OTP,
                                    IsLimitExceeded = false,
                                    IsTransactionNeedOTP = true,
                                    ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_OTP, language)}"
                                }

                            };
                            return response;
                        }

                        else if (request.Amount >= MaxSingleTransactionWithToken)
                        {

                            response = new ServiceResponseT<TransferResponse>(false)
                            {
                                FaultType = FaultMode.LIMIT_EXCEED_FOR_TOKEN,
                                Error = new ErrorResponseT
                                {
                                    ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_TOKEN,
                                    IsLimitExceeded = false,
                                    IsTransactionNeedOTP = true,
                                    ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_TOKEN, language)}"
                                }

                            };
                            return response;
                        }
                        else if (request.Amount >= MaxDailyTransactionWithToken)
                        {

                            response = new ServiceResponseT<TransferResponse>(false)
                            {
                                FaultType = FaultMode.LIMIT_EXCEED_FOR_TOKEN,
                                Error = new ErrorResponseT
                                {
                                    ResponseCode = ResponseCodes.LIMIT_EXCEED_FOR_TOKEN,
                                    IsLimitExceeded = false,
                                    IsTransactionNeedOTP = true,
                                    ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEED_FOR_TOKEN, language)}"
                                }

                            };
                            return response;
                        }
                        else
                        {

                        }



                    }
                }
                else
                {

                }
                //Added newly
                _logger.LogInformation("Returns from  the _limitService.ValidateLimit  inside AccountToWallet  method of NationalTransferService");

                if (!string.IsNullOrEmpty(request.Narration))
                {
                    request.Narration = Util.EncodeString($"{_settings.NarrationPrefix}{request.Narration}");
                }

                var transaction = new Transaction
                {
                    Amount = request.Amount,
                    CustomerId = user.Id,
                    DateCreated = DateTime.Now,
                    Narration = request.Narration,
                    TransactionReference = transactionReference,
                    SourceAccountId = request.SourceAccountId,
                    TransactionType = TransactionType.NationalTransfer,
                    DestinationAccountID = request.DestinationAccountId,
                    TransactionStatus = TransactionStatus.New,
                    ResponseCode = FI_ERROR //This line is logged because FI lost in transit without returning response
                };

                _logger.LogInformation(
                    "Start calling _transactionDAO.Add inside the AccountToWallet method of the NAtionalTransferService : Request===>{0}",
                    JsonConvert.SerializeObject(transaction));


                transaction.ID = await _transactionDAO.Add(transaction);
                _logger.LogInformation(
                    "Finished calling _transactionDAO.Add inside the AccountToWallet method of the NAtionalTransferService. Response ======>{0}",
                    transaction.ID);
                //TODO: Add check to confirm that the source account belongs to the user
                if (!string.IsNullOrEmpty(request.SourceAccountName))
                {
                    request.SourceAccountName = Util.EncodeString(request.SourceAccountName);
                }

                var mirrorTransfer = new BaseTransferRequest()
                {
                    Amount = request.Amount,
                    DestinationAccountId = _settings.WalletFundingAccount?.AccountNumber,
                    Narration = request.Narration,
                    SourceAccountName = request.SourceAccountName,
                    SourceAccountId = request.SourceAccountId
                };

                BasicResponse mirrorTransferResponse = new BasicResponse();
                try
                {
                    _logger.LogInformation(
                        "Starting calling  _localService.Transfer service method inside the AccountToWallet method of the NAtionalTransferService. Request ======>{0} Reference===>{1}",
                        JsonConvert.SerializeObject(mirrorTransfer), transactionReference);

                    mirrorTransferResponse = await _localService.Transfer(mirrorTransfer, transactionReference);
                    _logger.LogInformation(
                        "ACCOUNT_TO_WALLET ACCOUNT_DEBIT FOR TRANSACTION_REFERENCE: {0} ------ RESPONSE: {1}",
                        transactionReference, JsonConvert.SerializeObject(mirrorTransferResponse));

                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Server error occurred:EXCEPTION WHILE CALLING FI");

                    transaction.TransactionStatus = TransactionStatus.FIException;
                    transaction.ResponseCode = mirrorTransferResponse.Error?.ResponseCode;
                    if (string.IsNullOrEmpty(transaction.ResponseCode))
                    {
                        transaction.ResponseCode = NRFI_ERROR;
                    }

                    transaction.ResponseTime = DateTime.Now;
                    await _transactionDAO.Update(transaction);

                    response.FaultType = FaultMode.GATEWAY_ERROR;
                    response.Error = new ErrorResponseT()
                    {
                        ResponseCode = ResponseCodes.GENERAL_ERROR,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                    };
                    if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
                    {
                        response.Error = new ErrorResponseT()
                        {
                            ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                            ResponseDescription =
                                _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                        };
                        return response;
                    }

                    return response;
                }

                if (mirrorTransferResponse.IsSuccessful == false)
                {
                    transaction.TransactionStatus = TransactionStatus.Failed;
                    transaction.ResponseCode = mirrorTransferResponse.Error.ResponseCode;
                    transaction.ResponseTime = DateTime.Now;
                    await _transactionDAO.Update(transaction);
                    response.Error = new ErrorResponseT()
                    {
                        ResponseCode = mirrorTransferResponse.Error.ResponseCode,
                        ResponseDescription =
                            _messageProvider.GetMessage(mirrorTransferResponse.Error.ResponseCode, language)
                    };

                    return response;
                }

                BasicResponse walletFundingResponse = new BasicResponse();


                try
                {

                    walletFundingResponse = await _walletService.FundWallet(request.DestinationAccountId,
                        request.Amount, request.Narration, transactionReference);
                    _logger.LogInformation(
                        "ACCOUNT_TO_WALLET WALLET_CREDIT FOR TRANSACTION_REFERENCE: {0} ------ RESPONSE: {1}",
                        transactionReference, JsonConvert.SerializeObject(walletFundingResponse));
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "EXCEPTION WHILE CALLING WALLET SERVICE: {0}");
                    transaction.TransactionStatus = TransactionStatus.WalletException;
                    transaction.ResponseCode = walletFundingResponse.Error?.ResponseCode;
                    transaction.ResponseTime = DateTime.Now;
                    await _transactionDAO.Update(transaction);

                    response.FaultType = FaultMode.GATEWAY_ERROR;

                    response.Error = new ErrorResponseT()
                    {
                        ResponseCode = ResponseCodes.GENERAL_ERROR,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                    };
                    if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
                    {
                        response.Error = new ErrorResponseT()
                        {
                            ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                            ResponseDescription =
                                _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                        };
                        return response;
                    }

                    return response;
                }

                transaction.ResponseTime = DateTime.Now;

                if (walletFundingResponse.IsSuccessful == false)
                {
                    transaction.TransactionStatus = TransactionStatus.Failed;
                    transaction.ResponseCode = walletFundingResponse.Error.ResponseCode;
                    transaction.ResponseTime = DateTime.Now;
                    await _transactionDAO.Update(transaction);

                    await LogForReversal(transaction.ID, ReversalType.Account);
                    _logger.LogInformation("REVERSAL LOGGED FOR TRANSACTION_REFERENCE: {0} ------ transaction.ID :{1}",
                        transactionReference, transaction.ID);

                    response.Error = new ErrorResponseT()
                    {
                        ResponseCode = walletFundingResponse.Error.ResponseCode,
                        ResponseDescription =
                            _messageProvider.GetMessage(walletFundingResponse.Error.ResponseCode, language)
                    };

                    return response;
                }


                transaction.ResponseCode = WALLET_SUCCESS_CODE;
                transaction.TransactionStatus = TransactionStatus.Successful;

                await _transactionDAO.Update(transaction);

                response.IsSuccessful = true;

                var transferResponse = new TransferResponse
                {
                    Date = DateTime.Now,
                    Reference = transactionReference,
                    BeneficiaryStatus = new BeneficiaryStatus
                    {
                        Attempted = saveAsBeneficiary,
                    },
                    TransactionDetails = new TransactionDetails
                    {
                        Amount = request.Amount,
                        DestinationAccountName = request.DestinationAccountName,
                        DestinationAccountID = request.DestinationAccountId,
                        Narration = Util.DecodeString(request.Narration),
                        SourceAccountName = Util.DecodeString(request.SourceAccountName),
                        SourceAccountNumber = request.SourceAccountId,
                        DestinationBank = ""
                    }
                };
                _logger.LogInformation(
                    "Checking if the saveAsBeneficiary property is being returned  as expected: saveAsBeneficiary:===>{0}",
                    saveAsBeneficiary);
                if (saveAsBeneficiary == true)
                {

                    if (!string.IsNullOrEmpty(request.DestinationAccountName))
                    {
                        request.DestinationAccountName = Util.EncodeString(request.DestinationAccountName);
                    }

                    if (string.IsNullOrEmpty(request.DestinationInstitutionId))
                    {
                        request.DestinationInstitutionId = _settings?.WalletCode;
                    }

                    var beneficaryStatus = await SaveBeneficiary(request, user.UserName, AccountType.WALLET);

                    transferResponse.BeneficiaryStatus.IsSuccessful = beneficaryStatus.IsSuccessful;
                    var beneiciaryResponseCode = beneficaryStatus.IsSuccessful
                        ? ResponseCodes.BENEFICIARY_SAVED_SUCCESSFULLY
                        : ResponseCodes.UNABLE_TO_SAVE_BENEFICIARY;
                    transferResponse.BeneficiaryStatus.Message =
                        _messageProvider.GetMessage(beneiciaryResponseCode, language);
                    _logger.LogInformation("SaveBeneficiary response:===>{0}",
                        JsonConvert.SerializeObject(beneficaryStatus));
                }

                response.SetPayload(transferResponse);
                _logger.LogInformation("General response:===>{0}",
                       JsonConvert.SerializeObject(transferResponse));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error occurred inside the AccountToWallet Method of the NationalTransferService");
                return response;
            }
        }

        public async Task<ServiceResponse<TransferResponse>> WalletToAccount(AuthenticatedTransferRequest request, AuthenticatedUser user, string language, bool saveAsBeneficiary)
        {
            _logger.LogInformation("Inside the WalletToAccount in the NationalTransferservice ");
            if (string.IsNullOrWhiteSpace(request.Narration))
            {
                request.Narration = null;
            }
            var response = new ServiceResponse<TransferResponse>(false);
            try
            {

                if (!request.IsValid(out var source))
                {
                    response = new ServiceResponse<TransferResponse>(false)
                    {
                        FaultType = FaultMode.CLIENT_INVALID_ARGUMENT,
                        Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                            ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}"
                        }
                    };
                    return response;
                }
                _logger.LogInformation("Calling the _authenticator.ValidatePin ");
                var validationResponse = await _authenticator.ValidatePin(user.UserName, request.Pin);
                if (!validationResponse.IsSuccessful)
                {
                    response = new ServiceResponse<TransferResponse>(false)
                    {
                        FaultType = FaultMode.UNAUTHORIZED,
                        Error = new ErrorResponse
                        {
                            ResponseCode = validationResponse.Error?.ResponseCode,
                            ResponseDescription = _messageProvider.GetMessage(validationResponse.Error?.ResponseCode, language)
                        }
                    };
                    return response;
                }
                var transactionReference = DateTime.Now.Ticks.ToString();

                if (!string.IsNullOrEmpty(request.Narration))
                {
                    request.Narration = Util.EncodeString($"{_settings.NarrationPrefix}{request.Narration}");
                }
             

                var transaction = new Transaction
                {
                    Amount = request.Amount,
                    CustomerId = user.Id,
                    DateCreated = DateTime.Now,
                    Narration = request.Narration,
                    TransactionReference = transactionReference,
                    SourceAccountId = request.SourceAccountId,
                    TransactionType = TransactionType.NationalTransfer,
                    DestinationAccountID = request.DestinationAccountId,
                    TransactionStatus = TransactionStatus.New,
                    DestinationTransactionTag = TransactionTag.New,
                    SourceTransactionTag = TransactionTag.New, 
                    ResponseCode = FI_ERROR //This line is logged because FI lost in transit without returning response
                };
                _logger.LogInformation(" Started Calling the _transactionDAO.Add . Payload:===>{0}", JsonConvert.SerializeObject(transaction));
                transaction.ID = await _transactionDAO.Add(transaction);
                _logger.LogInformation(" finished Calling the _transactionDAO.Add . Response:===>{0}", transaction.ID);

                BasicResponse walletChargingResponse = new BasicResponse();

                try
                {
                    _logger.LogInformation(" Start Calling the _walletService.ChargeWallet");
                    walletChargingResponse = await _walletService.ChargeWallet(request.SourceAccountId, request.Amount, request.Narration, transactionReference);
                    _logger.LogInformation("WALLET_TO_ACCOUNT WALLET DEBIT FOR TRANSACTION_REFERENCE: {0} ------ RESPONSE:{1}", transactionReference, JsonConvert.SerializeObject(walletChargingResponse));
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "EXCEPTION WHILE CALLING WALLET SERVICE");
                    transaction.TransactionStatus = TransactionStatus.WalletException;
                    transaction.ResponseCode = walletChargingResponse.Error?.ResponseCode;
                    transaction.ResponseTime = DateTime.Now;
                    await _transactionDAO.Update(transaction);

                    response.FaultType = FaultMode.GATEWAY_ERROR;
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = ResponseCodes.GENERAL_ERROR,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                    };
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
                    return response;
                }


                if (walletChargingResponse.IsSuccessful == false)
                {
                    transaction.TransactionStatus = TransactionStatus.Failed;
                    transaction.ResponseCode = walletChargingResponse.Error.ResponseCode;
                    transaction.ResponseTime = DateTime.Now;
                    await _transactionDAO.Update(transaction);
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = walletChargingResponse.Error.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(walletChargingResponse.Error.ResponseCode, language)
                    };

                    return response;
                }

                if (!string.IsNullOrEmpty(request.DestinationAccountName))
                {
                    request.DestinationAccountName = Util.EncodeString(request.DestinationAccountName);
                }
                var accountTransfer = new BaseTransferRequest()
                {
                    Amount = request.Amount,
                    SourceAccountId = _settings.WalletChargingAccount?.AccountNumber,
                    Narration = request.Narration,
                    DestinationAccountName = request.DestinationAccountName,
                    DestinationAccountId = request.DestinationAccountId,
                };

                BasicResponse accountTransferResponse = new BasicResponse();
                try
                {
                    _logger.LogInformation("Started to call the _localService.Transfer from WalletToAccount in the NationalTransafer service");
                    accountTransferResponse = await _localService.Transfer(accountTransfer, transactionReference);
                    _logger.LogInformation("ACCOUNT_CREDIT FOR WALLET_TO_ACCOUNT TRANSACTION_REFERENCE: {0} ------ RESPONSE: {1} ", transactionReference, JsonConvert.SerializeObject(accountTransferResponse));
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "EXCEPTION WHILE CALLING FI");
                    transaction.TransactionStatus = TransactionStatus.FIException;
                    transaction.ResponseCode = accountTransferResponse.Error?.ResponseCode;
                    if (string.IsNullOrEmpty(transaction.ResponseCode))
                    {
                        transaction.ResponseCode = NRFI_ERROR;
                    }
                    transaction.ResponseTime = DateTime.Now;
                    await _transactionDAO.Update(transaction);

                    response.FaultType = FaultMode.GATEWAY_ERROR;
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = ResponseCodes.GENERAL_ERROR,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                    };
                    if (ex.Message.Contains("A connection attempt failed"))
                    {
                        response.Error = new ErrorResponse()
                        {
                            ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                            ResponseDescription =
                                _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                        };
                        return response;
                    }
                    _logger.LogInformation("Transaction failed {error}.  Error occurred", JsonConvert.SerializeObject(accountTransferResponse));
                    return response;
                }

                transaction.ResponseTime = DateTime.Now;

                if (accountTransferResponse.IsSuccessful==false)
                {
                    transaction.TransactionStatus = TransactionStatus.Failed;
                    transaction.ResponseCode = accountTransferResponse.Error.ResponseCode;
                    if (string.IsNullOrEmpty(transaction.ResponseCode))
                    {
                        transaction.ResponseCode = NRFI_ERROR;
                    }
                    _logger.LogInformation("Transaction failed {error}",JsonConvert.SerializeObject(accountTransferResponse));
                    await _transactionDAO.Update(transaction);

                    await LogForReversal(transaction.ID, ReversalType.Wallet);
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = accountTransferResponse.Error.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(accountTransferResponse.Error.ResponseCode, language)
                    };

                    return response;
                }

                transaction.TransactionStatus = TransactionStatus.Successful;
                transaction.ResponseCode = TRANSFER_SUCCESS_CODE;

                await _transactionDAO.Update(transaction);

                response.IsSuccessful = true;

                var transferResponse = new TransferResponse
                {
                    Date = DateTime.Now,
                    Reference = transactionReference,
                    BeneficiaryStatus = new BeneficiaryStatus
                    {
                        Attempted = saveAsBeneficiary,
                    },
                    TransactionDetails = new TransactionDetails
                    {
                        Amount = request.Amount,
                        DestinationAccountName = Util.DecodeString(request.DestinationAccountName),
                        DestinationAccountID = request.DestinationAccountId,
                        Narration = Util.DecodeString(request.Narration),
                        SourceAccountName = request.SourceAccountName,
                        SourceAccountNumber = request.SourceAccountId,
                        DestinationBank = request.DestinationInstitutionId,
                    }
                };

                _logger.LogInformation("Savebeneficiary:===>{0}", saveAsBeneficiary);
                if (saveAsBeneficiary)
                {
                    var beneficaryStatus = await SaveBeneficiary(request, user.UserName, AccountType.BANK);

                    transferResponse.BeneficiaryStatus.IsSuccessful = beneficaryStatus.IsSuccessful;
                    var beneiciaryResponseCode = beneficaryStatus.IsSuccessful ? ResponseCodes.BENEFICIARY_SAVED_SUCCESSFULLY : ResponseCodes.UNABLE_TO_SAVE_BENEFICIARY;
                    transferResponse.BeneficiaryStatus.Message = _messageProvider.GetMessage(beneiciaryResponseCode, language);
                    _logger.LogInformation("Savebeneficiary Response:===>{0}", JsonConvert.SerializeObject(transferResponse));

                }

                response.SetPayload(transferResponse);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error occurred inside the WalletToAccount Method of the NationalTransferService");
                return response;
            }
        }

        private async Task LogForReversal(long transactionId, ReversalType reversalType)
        {
            try
            {
                _logger.LogInformation("Inside LogForReversal of the National Transfer Service");
                await _reversalDAO.Add(new Middleware.Core.Model.Reversal
                {
                    TransactionId = transactionId,
                    ReversalStatus = ReversalStatus.Pending,
                    ReversalType = reversalType
                });
                _logger.LogInformation("REVERSAL LOGGED FOR TRANS ID: {0}", transactionId);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Error logging transaction for reversal : TransactionId {0}", transactionId);
            }
        }

        //PLEASE DO NOT REMOVE THIS COMMENTED CODE. I FEAR THE BUSINESS MIGHT TURN AROUND AND ASK FOR THIS AGAIN 
        //public async Task<ServiceResponse<TransferResponse>> WalletToWallet(AuthenticatedTransferRequest request, AuthenticatedUser user, string language, bool saveAsBeneficiary)
        //{
        //    var response = new ServiceResponse<TransferResponse>(false);
        //    if (!request.IsValid(out var source))
        //    {
        //        response = new ServiceResponse<TransferResponse>(false)
        //        {
        //            FaultType = FaultMode.CLIENT_INVALID_ARGUMENT,
        //            Error = new ErrorResponse
        //            {
        //                ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
        //                ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}"
        //            }
        //        };
        //        return response;
        //    }

        //    var validationResponse = await _authenticator.ValidatePin(user.UserName, request.Pin);
        //    if (!validationResponse.IsSuccessful)
        //    {
        //        response = new ServiceResponse<TransferResponse>(false)
        //        {
        //            FaultType = FaultMode.UNAUTHORIZED,
        //            Error = new ErrorResponse
        //            {
        //                ResponseCode = ResponseCodes.INVALID_TRANSACTION_PIN,
        //                ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_TRANSACTION_PIN, language)
        //            }
        //        };
        //        return response;
        //    }
        //    var transactionReference = Guid.NewGuid().ToString();


        //    request.Narration = $"{_settings.NarrationPrefix}{request.Narration}";

        //    var transaction = new Transaction
        //    {
        //        Amount = request.Amount,
        //        CustomerId = user.Id,
        //        DateCreated = DateTime.Now,
        //        Narration = request.Narration,
        //        TransactionReference = transactionReference,
        //        SourceAccountId = request.SourceAccountId,
        //        TransactionType = TransactionType.NationalTransfer,
        //        DestinationAccountID = request.DestinationAccountId,
        //        TransactionStatus = TransactionStatus.New
        //    };

        //    await _transactionDAO.Add(transaction);
        //    //TODO: Add check to confirm that the source account belongs to the user

        //    var walletTransferResponse = await _walletService.WalletTransfer(request);

        //    if (!walletTransferResponse.IsSuccessful)
        //    {
        //        response.Error = new ErrorResponse()
        //        {
        //            ResponseCode = walletTransferResponse.Error.ResponseCode,
        //            ResponseDescription = _messageProvider.GetMessage(walletTransferResponse.Error.ResponseCode, language)
        //        };

        //        return response;

        //    }

        //    transaction.ResponseTime = DateTime.Now;
        //    transaction.ResponseCode = walletTransferResponse.IsSuccessful ? "00" : walletTransferResponse.Error.ResponseCode;
        //    transaction.TransactionStatus = walletTransferResponse.IsSuccessful ? TransactionStatus.Successful : TransactionStatus.Failed;

        //    await _transactionDAO.Update(transaction);

        //    response.IsSuccessful = true;

        //    var transferResponse = new TransferResponse
        //    {
        //        Date = DateTime.Now.Date,
        //        Reference = transactionReference,
        //        BeneficiaryStatus = new BeneficiaryStatus
        //        {
        //            Attempted = saveAsBeneficiary,
        //        },
        //        TransactionDetails = new TransactionDetails
        //        {
        //            Amount = request.Amount,
        //            DestinationAccountName = request.DestinationAccountName,
        //            DestinationAccountID = request.DestinationAccountId,
        //            Narration = request.Narration,
        //            SourceAccountName = request.SourceAccountName,
        //            SourceAccountNumber = request.SourceAccountId,
        //            DestinationBank = "",  
        //        }
        //    };


        //    if (saveAsBeneficiary)
        //    {
        //        var beneficaryStatus = await SaveBeneficiary(request, user.UserName, AccountType.WALLET);

        //        transferResponse.BeneficiaryStatus.IsSuccessful = beneficaryStatus.IsSuccessful;
        //        transferResponse.BeneficiaryStatus.Message = beneficaryStatus.IsSuccessful ? "Beneficiary saved successfully" : "Unable to save beneficiary";
        //    }

        //    response.SetPayload(transferResponse);
        //    return response;
        //}


        public async Task<ServiceResponse<TransferResponse>> WalletToWallet(AuthenticatedTransferRequest request, AuthenticatedUser user, string language, bool saveAsBeneficiary)
        {

            _logger.LogInformation("Inside the Wallet to wallet method of the NationalTransferService");
            _logger.LogInformation(" Wallet to Wallet Request {0}", JsonConvert.SerializeObject(new { request, user, language, saveAsBeneficiary }));
            if (string.IsNullOrWhiteSpace(request.Narration))
            {
                request.Narration = null;
            }
            var response = new ServiceResponse<TransferResponse>(false);
            if (!request.IsValid(out var source))
            {
                response = new ServiceResponse<TransferResponse>(false)
                {
                    FaultType = FaultMode.CLIENT_INVALID_ARGUMENT,
                    Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}"
                    }
                };
                return response;
            }

            var validationResponse = await _authenticator.ValidatePin(user.UserName, request.Pin);
            if (!validationResponse.IsSuccessful)
            {
                response = new ServiceResponse<TransferResponse>(false)
                {
                    FaultType = FaultMode.UNAUTHORIZED,
                    Error = new ErrorResponse
                    {
                        ResponseCode = validationResponse.Error?.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(validationResponse.Error?.ResponseCode, language)
                    }
                };
                return response;
            }


            var transactionReference = DateTime.Now.Ticks.ToString();

            if (!string.IsNullOrEmpty(request.Narration))
            {
                request.Narration = Util.EncodeString($"{_settings.NarrationPrefix}{request.Narration}");
            }

            var transaction = new Transaction
            {
                Amount = request.Amount,
                CustomerId = user.Id,
                DateCreated = DateTime.Now,
                Narration = request.Narration,
                TransactionReference = transactionReference,
                SourceAccountId = request.SourceAccountId,
                TransactionType = TransactionType.NationalTransfer,
                DestinationAccountID = request.DestinationAccountId,
                TransactionStatus = TransactionStatus.New,
                DestinationTransactionTag = TransactionTag.New,
                SourceTransactionTag = TransactionTag.New,
                ResponseCode = FI_ERROR //This line is logged because FI lost in transit without returning response
            };

            transaction.ID = await _transactionDAO.Add(transaction);

           // #region New_WalletTransaction
            _logger.LogInformation("WALLET_TRANSFER_REQUEST: {0}", JsonConvert.SerializeObject(request));
            var isWithinLimit = await _limitService.ValidateLimit(user.Id, TransactionType.InterWalletTransfer, request.Amount);
            if (isWithinLimit.NotFound == true)
            {
                _logger.LogInformation("transaction limit has not been set it returns null ");
                response.Error = new ErrorResponse()
                {
                    ResponseCode = ResponseCodes.TRANSACTION_LIMIT_NOT_YET_SET,
                    ResponseDescription =
                        _messageProvider.GetMessage(ResponseCodes.TRANSACTION_LIMIT_NOT_YET_SET, language)
                };
                return response;

            }
            if (isWithinLimit.IsLimitExceeded == true)
            {
                response = new ServiceResponse<TransferResponse>(false)
                {
                    FaultType = FaultMode.LIMIT_EXCEEDED,
                    Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.LIMIT_EXCEEDED,
                        ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEEDED, language)}"
                    }
                };
                return response;
            }
            //Used before now mutted on the 6/7/2021
            if (!string.IsNullOrEmpty(request.DestinationInstitutionId) && request.DestinationInstitutionId != _settings.WalletCode)
            {
                // process inter wallet transaction
                //var isWithinLimit = await _limitService.ValidateLimit(user.Id, TransactionType.InterWalletTransfer, request.Amount);
                //if (isWithinLimit.IsLimitExceeded)
                //{
                //    response = new ServiceResponse<TransferResponse>(false)
                //    {
                //        FaultType = FaultMode.LIMIT_EXCEEDED,
                //        Error = new ErrorResponse
                //        {
                //            ResponseCode = ResponseCodes.LIMIT_EXCEEDED,
                //            ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEEDED, language)}"
                //        }
                //    };
                //    return response;
                //}
                response = await ProcessInterWalletTransaction(request, transaction, transactionReference, language);
                _logger.LogInformation("WALLET_TRANSFER_RESPONSEEEE:{0}", JsonConvert.SerializeObject(response));
                if (!response.IsSuccessful)
                {
                    return response;
                }
            }
            else
            {
                //process local wallet transaction
                response = await ProcessWalletTransaction(request, transaction, transactionReference, language);
                _logger.LogInformation("WALLET_TRANSFER_RESPONSEEEE:{0}", JsonConvert.SerializeObject(response));
                if (!response.IsSuccessful)
                {
                    return response;
                }
            }
           // #endregion

          #region OldWalletTransaction
            //BasicResponse walletChargingResponse;
            //try
            //{
            //    walletChargingResponse = await _walletService.ChargeWallet(request.SourceAccountId,
            //                   request.Amount, request.Narration, transactionReference);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError($"EXCEPTION WHILE CALLING WALLET SERVICE: {ex}");
            //    transaction.TransactionStatus = TransactionStatus.Failed;
            //    await _transactionDAO.Update(transaction);

            //    response.FaultType = FaultMode.GATEWAY_ERROR;
            //    response.Error = new ErrorResponse()
            //    {
            //        ResponseCode = ResponseCodes.GENERAL_ERROR,
            //        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
            //    };

            //    return response;
            //}

            //if (!walletChargingResponse.IsSuccessful)
            //{
            //    transaction.TransactionStatus = TransactionStatus.Failed;
            //    transaction.ResponseCode = walletChargingResponse.Error.ResponseCode;
            //    await _transactionDAO.Update(transaction);
            //    response.Error = new ErrorResponse()
            //    {
            //        ResponseCode = walletChargingResponse.Error.ResponseCode,
            //        ResponseDescription = _messageProvider.GetMessage(walletChargingResponse.Error.ResponseCode, language)
            //    };

            //    return response;

            //}

            //BasicResponse walletFundingResponse;

            //try
            //{
            //    walletFundingResponse = await _walletService.FundWallet(request.DestinationAccountId, request.Amount, request.Narration, $"{transactionReference}-1");//  Cant use the same reference since the transaction is going to the same processor

            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError($"EXCEPTION WHILE CALLING WALLET SERVICE: {ex}");
            //    transaction.TransactionStatus = TransactionStatus.WalletException;
            //    await _transactionDAO.Update(transaction);

            //    response.FaultType = FaultMode.GATEWAY_ERROR;
            //    response.Error = new ErrorResponse()
            //    {
            //        ResponseCode = ResponseCodes.GENERAL_ERROR,
            //        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
            //    };

            //    return response;
            //}

            //if (!walletFundingResponse.IsSuccessful)
            //{
            //    transaction.TransactionStatus = TransactionStatus.Failed;
            //    transaction.ResponseCode = walletChargingResponse.Error.ResponseCode;
            //    await _transactionDAO.Update(transaction);
            //    await LogForReversal(transaction.ID, ReversalType.Wallet);
            //    response.Error = new ErrorResponse()
            //    {
            //        ResponseCode = walletFundingResponse.Error.ResponseCode,
            //        ResponseDescription = _messageProvider.GetMessage(walletFundingResponse.Error.ResponseCode, language)
            //    };

            //    return response;
            //}
            #endregion


            transaction.ResponseTime = DateTime.Now;
            transaction.ResponseCode = WALLET_SUCCESS_CODE;
            transaction.TransactionStatus = TransactionStatus.Successful;

            await _transactionDAO.Update(transaction);

            response.IsSuccessful = true;

            var transferResponse = new TransferResponse
            {
                Date = DateTime.Now,
                Reference = transactionReference,
                BeneficiaryStatus = new BeneficiaryStatus
                {
                    Attempted = saveAsBeneficiary,
                },
                TransactionDetails = new TransactionDetails
                {
                    Amount = request.Amount,
                    DestinationAccountName = request.DestinationAccountName,
                    DestinationAccountID = request.DestinationAccountId,
                    Narration = Util.DecodeString(request.Narration),
                    SourceAccountName = request.SourceAccountName,
                    SourceAccountNumber = request.SourceAccountId,
                    DestinationBank = "",
                }
            };

            _logger.LogInformation("saveAsBeneficiary=====>{0}", saveAsBeneficiary);
            if (saveAsBeneficiary)
            {
                if (!string.IsNullOrEmpty(request.DestinationAccountName))
                {
                    request.DestinationAccountName = Util.EncodeString(request.DestinationAccountName);
                }
                if (string.IsNullOrEmpty(request.DestinationInstitutionId))
                {
                    request.DestinationInstitutionId = _settings.WalletCode;
                }
                var beneficaryStatus = await SaveBeneficiary(request, user.UserName, AccountType.WALLET);

                transferResponse.BeneficiaryStatus.IsSuccessful = beneficaryStatus.IsSuccessful;
                transferResponse.BeneficiaryStatus.Message = beneficaryStatus.IsSuccessful == true ? _messageProvider.GetMessage(ResponseCodes.BENEFICIARY_SAVED_SUCCESSFULLY, language) : _messageProvider.GetMessage(ResponseCodes.UNABLE_TO_SAVE_BENEFICIARY, language);
                _logger.LogInformation("saveAsBeneficiary  Response :=====>{0}", JsonConvert.SerializeObject(transferResponse));
            }

            response.SetPayload(transferResponse);
            return response;
        }

        private async Task<BasicResponse> SaveBeneficiary(BaseTransferRequest request, string customerID, AccountType type)
        {
            _logger.LogInformation("Inside the SaveBeneficiary method of Nationaltransfer Service");
            _logger.LogInformation("  SaveBeneficiary Request {0}", JsonConvert.SerializeObject(new { request, customerID, type }));


            if (string.IsNullOrWhiteSpace(request.Narration))
            {
                request.Narration = null;
            }
            var response = new BasicResponse(false);
            var beneficiary = new TransferBeneficiary
            {
                AccountName = request.DestinationAccountName,
                AccountId = request.DestinationAccountId,
                InstitutionCode = request.DestinationInstitutionId,
                Alias = request.DestinationAccountName,
                AccountType = type
            };

            try
            {

                response = await _beneficiaryService.AddTransferBeneficiary(beneficiary, customerID);

                if (response.Error?.ResponseCode == "BMS040")
                {

                    response.Error.ResponseCode = ResponseCodes.BENEFICIARY_ALREADY_EXISTS;
                }
                _logger.LogInformation("Response gotten from  SaveBenefitiary  Method of NationaltransferService  on a call to  _beneficiaryService.AddTransferBeneficiary : {0}", JsonConvert.SerializeObject(response));
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Could not save beneficiary after transfer. account number: {0}",
                    new { request.DestinationAccountId, customerID });
            }

            return response;

        }

        public async Task<InstitutionResult> GetInstitutions()
        {
            _logger.LogInformation("Inside the GetInstitutions of the NationalTransferService");
            var result = new InstitutionResult();
            var institutions = await _institutionDAO.FindByStatus(true);



            result.Banks = institutions.Where(i => i.Category == InstitutionCategory.BANK).
                                        Select(s => new InstitutionDetails
                                        {
                                            Name = s.InstitutionName.Replace("DIAMOND", "NSia"),
                                            Code = s.InstitutionCode
                                        }
                                        ).Where(p => p.Name.ToLower() != "fbn senegal"
                                        && p.Name.ToLower() != "fbn bank france" &&
                                        p.Name.ToLower() != "fbnbank senegal")
                                        .DistinctBy(p => p.Name)
                                        .DistinctBy(p => p.Code)
                                            .ToList().OrderBy(p => p.Name);

            result.MobileMoneyOperators = institutions.Where(i => i.Category == InstitutionCategory.MOBILE_MONEY).
                Select(s => new InstitutionDetails
                {
                    Name = s.InstitutionName.Replace("DIAMOND", "NSia"),
                    Code = s.InstitutionCode
                }
                ).DistinctBy(p => p.Name)
                .DistinctBy(p => p.Code).ToList();

            _logger.LogInformation("Response from the _institutionDAO.FindByStatus in  the GetInstitutions of the NationalTransferService {0}", JsonConvert.SerializeObject(result));
            //_logger.LogInformation($"SEE_INSTITUTIONS: {JsonConvert.SerializeObject(result)}");
            return result;
        }

        public async Task<ServiceResponse<TransferResponse>> TransferToSelf(SelfTransferRequest request, AuthenticatedUser user, string language)
        {
            _logger.LogInformation("  Transfer to Self  Request {0}", JsonConvert.SerializeObject(request));
            if (string.IsNullOrWhiteSpace(request.Narration))
            {
                request.Narration = null;
            }
            var response = new ServiceResponse<TransferResponse>(false);
            if (!request.Validate(out var source))
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}"
                };
                return response;
            }
            var accountsMatchR = (await _localService.DoAccountsBelongtoCustomer(user.BankId, request.SourceAccountNumber, request.DestinationAccountNumber));

            _logger.LogInformation("Response from the _localService.DoAccountsBelongtoCustomer in  the TransferToSelf of the NationalTransferService  . Reponse==========>{0}", JsonConvert.SerializeObject(accountsMatchR));
            var accountsMatch = (accountsMatchR).IsSuccessful;
            if (!accountsMatch)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH, language)
                };
                return response;
            }
            var transactionReference = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(request.Narration))
            {
                request.Narration = Util.EncodeString($"{_settings.NarrationPrefix}{request.Narration}");
            }
            var transaction = new Transaction()
            {
                Amount = request.Amount,
                CustomerId = user.Id,
                DateCreated = DateTime.Now,
                Narration = request.Narration,
                TransactionReference = transactionReference,
                SourceAccountId = request.SourceAccountNumber,
                TransactionType = TransactionType.SelfTransfer,
                DestinationAccountID = request.DestinationAccountNumber,
                TransactionStatus = TransactionStatus.New,
                ResponseCode = FI_ERROR //This line is logged because FI lost in transit without returning response
            };

            await _transactionDAO.Add(transaction);

            _logger.LogInformation("Response from the _transactionDAO.Add in  the TransferToSelf of the NationalTransferService");

            BasicResponse paymentResponse = new BasicResponse();
            try
            {
                paymentResponse = await _localService.TransferToSelf(request, transactionReference);

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "EXCEPTION WHILE CALLING FI:");
                transaction.TransactionStatus = TransactionStatus.FIException;
                transaction.ResponseCode = paymentResponse.Error?.ResponseCode;
                if (string.IsNullOrEmpty(transaction.ResponseCode))
                {
                    transaction.ResponseCode = NRFI_ERROR;
                }
                transaction.ResponseTime = DateTime.Now;
                await _transactionDAO.Update(transaction);

                response.FaultType = FaultMode.GATEWAY_ERROR;
                response.Error = new ErrorResponse()
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };
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
                return response;
            }

            transaction.ResponseTime = DateTime.Now;
            transaction.ResponseCode = paymentResponse.IsSuccessful ? TRANSFER_SUCCESS_CODE : paymentResponse.Error.ResponseCode;
            if (string.IsNullOrEmpty(transaction.ResponseCode))
            {
                transaction.ResponseCode = NRFI_ERROR;
            }
            transaction.TransactionStatus = paymentResponse.IsSuccessful ? TransactionStatus.Successful : TransactionStatus.Failed;

            await _transactionDAO.Update(transaction);

            if (!paymentResponse.IsSuccessful)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH,
                    ResponseDescription = _messageProvider.GetMessage(paymentResponse.Error.ResponseCode, language)
                };
                return response;
            }

            response.IsSuccessful = true;

            response.SetPayload(new TransferResponse
            {
                Date = DateTime.Now,
                Reference = transactionReference,
                BeneficiaryStatus = new BeneficiaryStatus
                {
                    Attempted = false,
                    IsSuccessful = false,
                    Message = ""
                },
                TransactionDetails = new TransactionDetails
                {
                    Amount = request.Amount,
                    DestinationAccountID = request.DestinationAccountNumber,
                    Narration = Util.DecodeString(request.Narration),
                    SourceAccountNumber = request.SourceAccountNumber
                }
            });
            _logger.LogInformation("Response from the TransferToSelf in   of the NationalTransferService {0}", JsonConvert.SerializeObject(accountsMatchR));
            return response;
        }

        public async Task<ServiceResponse<TransferResponse>> WalletToSelf(SelfTransferRequest request, AuthenticatedUser user, string language)
        {
            var response = new ServiceResponse<TransferResponse>(false);
            try
            {
                _logger.LogInformation("Inside the WalletToSelf method of the NationalTransferService at {0}", DateTime.UtcNow);
                _logger.LogInformation("  Wallet to Self  request {0}", JsonConvert.SerializeObject(request));


                if (string.IsNullOrWhiteSpace(request.Narration))
                {
                    request.Narration = null;
                }

                if (!request.Validate(out var source))
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}"
                    };
                    return response;
                }

                var transactionReference = DateTime.Now.Ticks.ToString();
                if (!string.IsNullOrEmpty(request.Narration))
                {
                    request.Narration = Util.EncodeString($"{_settings.NarrationPrefix}{request.Narration}");
                }
                var transaction = new Transaction()
                {
                    Amount = request.Amount,
                    CustomerId = user.Id,
                    DateCreated = DateTime.Now,
                    Narration = request.Narration,
                    TransactionReference = transactionReference,
                    SourceAccountId = request.SourceAccountNumber,
                    TransactionType = TransactionType.SelfTransfer,
                    DestinationAccountID = request.DestinationAccountNumber,
                    TransactionStatus = TransactionStatus.New,
                    ResponseCode = FI_ERROR //This line is logged because FI lost in transit without returning response
                };

                transaction.ID = await _transactionDAO.Add(transaction);

                var walletChargingResponse = await _walletService.ChargeWallet(request.SourceAccountNumber, request.Amount, request.Narration, transactionReference);
                _logger.LogInformation("WALLET_TO_SELF WALLET_DEBIT FOR TRANSACTION_REFERENCE: {0} ------ RESPONSE:{1}", transactionReference, JsonConvert.SerializeObject(walletChargingResponse));


                if (!walletChargingResponse.IsSuccessful)
                {
                    transaction.TransactionStatus = TransactionStatus.Failed;
                    transaction.ResponseCode = walletChargingResponse.Error?.ResponseCode;
                    transaction.ResponseTime = DateTime.Now;
                    await _transactionDAO.Update(transaction);

                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = walletChargingResponse.Error.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(walletChargingResponse.Error.ResponseCode, language)
                    };

                    return response;
                }

                if (!string.IsNullOrEmpty(request.DestinationAccountName))
                {
                    request.DestinationAccountName = Util.EncodeString(request.DestinationAccountName);
                }
                var accountTransfer = new BaseTransferRequest()
                {
                    Amount = request.Amount,
                    SourceAccountId = _settings.WalletChargingAccount?.AccountNumber,
                    Narration = request.Narration, //build appropiate narration
                    DestinationAccountName = request.DestinationAccountName,
                    DestinationAccountId = request.DestinationAccountNumber,
                };



                var accountTransferResponse = await _localService.Transfer(accountTransfer, transactionReference);
                _logger.LogInformation("ACCOUNT_CREDIT FOR WALLET_TO_SELF TRANSACTION_REFERENCE: {0} ------ RESPONSE: {1}", transactionReference, JsonConvert.SerializeObject(accountTransferResponse));

                transaction.ResponseTime = DateTime.Now;
                transaction.ResponseCode = accountTransferResponse.IsSuccessful ? TRANSFER_SUCCESS_CODE : accountTransferResponse.Error.ResponseCode;
                if (string.IsNullOrEmpty(transaction.ResponseCode))
                {
                    transaction.ResponseCode = NRFI_ERROR;
                }
                transaction.TransactionStatus = accountTransferResponse.IsSuccessful ? TransactionStatus.Successful : TransactionStatus.Failed;

                await _transactionDAO.Update(transaction);

                if (!accountTransferResponse.IsSuccessful)
                {
                    await LogForReversal(transaction.ID, ReversalType.Wallet);
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = accountTransferResponse.Error.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(accountTransferResponse.Error.ResponseCode, language)
                    };
                    return response;
                }

                response.IsSuccessful = true;
                response.SetPayload(new TransferResponse
                {
                    Date = DateTime.Now,
                    Reference = transactionReference,
                    BeneficiaryStatus = new BeneficiaryStatus
                    {
                        Attempted = false,
                        IsSuccessful = false,
                        Message = ""
                    },
                    TransactionDetails = new TransactionDetails
                    {
                        Amount = request.Amount,
                        DestinationAccountID = request.DestinationAccountNumber,
                        Narration = Util.DecodeString(request.Narration),
                        SourceAccountNumber = request.SourceAccountNumber
                    }
                });


                return response;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Inside the WalletToSelf method of the NationalTransferService at {0}", DateTime.UtcNow);
                response.Error = new ErrorResponse()
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };
                response.FaultType = FaultMode.SERVER;
                return response;
            }
        }


        public async Task<ServiceResponse<IEnumerable<Branch>>> GetBankBranches(string bankCode, string language)
        {
            try
            {
                var response = await _externalSrvice.GetBranches(bankCode);
                if (!response.IsSuccessful)
                {
                    response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                }

                _logger.LogInformation("Inside the GetBankBranches  method of the NationalTransferService ");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Inside the  GetBankBranches method of the NationalTransferService ");
                return new ServiceResponse<IEnumerable<Branch>>
                {
                    FaultType = FaultMode.SERVER,
                    Error = new ErrorResponse()
                    {
                        ResponseCode = ResponseCodes.GENERAL_ERROR,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                    },
                    IsSuccessful = false
                };
            }
            //TODO: Implement caching

        }

        public async Task<ServiceResponse<string>> GetWalletName(string walletId, string walletScheme, string language)
        {
            ServiceResponse<string> response;
            if (!string.IsNullOrEmpty(walletScheme))
            {
                // for other wallet schemes
                response = await _interWalletService.GetWalletName(walletId, walletScheme);
                //response = await _interWalletService.DoWalletNameValidation(walletId, walletScheme);
            }
            else
            {
                //for local wallet
                response = await _walletService.WalletEnquiry(walletId);
            }

            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                return response;
            }

            return response;
        }

        public async Task<ServiceResponse<IEnumerable<WalletSchemes>>> GetWalletSchemes(string language)
        {
            var response = new ServiceResponse<IEnumerable<WalletSchemes>>(false);

            response = await _interWalletService.GetSchemes();
            //response = await _interWalletService.GetWalletSchemes();

            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                return response;
            }

            return response;
        }

        public async Task<ServiceResponse<ServiceChargeResponse>> GetTransferCharge(decimal amount, decimal balance, string language)
        {
            var response = new ServiceResponse<ServiceChargeResponse>(false);

            response = await _interWalletService.GetServiceCharge(amount, balance);

            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                return response;
            }

            response.IsSuccessful = true;
            return response;
        }


        private async Task<ServiceResponse<TransferResponse>> ProcessWalletTransaction(AuthenticatedTransferRequest request, Transaction transaction, string transactionReference, string language)
        {
            if (string.IsNullOrWhiteSpace(request.Narration))
            {
                request.Narration = null;
            }
            var response = new ServiceResponse<TransferResponse>(false);
            BasicResponse walletChargingResponse = new BasicResponse();

            try
            {
                walletChargingResponse = await _walletService.ChargeWallet(request.SourceAccountId, request.Amount, request.Narration, transactionReference);
                _logger.LogInformation("WALLET_DEBIT_RESPONSE 11111111111:{0}", JsonConvert.SerializeObject(walletChargingResponse));
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Wallet first leg debit error. Reference: {0}", transactionReference);
                transaction.TransactionStatus = TransactionStatus.Failed;
                transaction.ResponseCode = $"{DEBIT_ERR_PREFIX}/{CEVA_NOTATION_PREFIX}/{walletChargingResponse.Error?.ResponseCode}";
                transaction.ResponseTime = DateTime.Now;
                await _transactionDAO.Update(transaction);

                response.FaultType = FaultMode.GATEWAY_ERROR;
                response.Error = new ErrorResponse()
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };

                return response;
            }

            if (!walletChargingResponse.IsSuccessful)
            {
                transaction.TransactionStatus = TransactionStatus.Failed;
                transaction.ResponseCode = walletChargingResponse.Error.ResponseCode;
                transaction.ResponseTime = DateTime.Now;
                await _transactionDAO.Update(transaction);
                response.Error = new ErrorResponse()
                {
                    ResponseCode = walletChargingResponse.Error.ResponseCode,
                    ResponseDescription = _messageProvider.GetMessage(walletChargingResponse.Error.ResponseCode, language)
                };

                return response;

            }

            BasicResponse walletFundingResponse = new BasicResponse();

            try
            {
                walletFundingResponse = await _walletService.FundWallet(request.DestinationAccountId, request.Amount, request.Narration, $"{transactionReference}-1");//  Cant use the same reference since the transaction is going to the same processor
                _logger.LogInformation("WALLET_FUNDING_RESPONSE 222222222222:{0}", JsonConvert.SerializeObject(walletFundingResponse));
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Wallet funding error. Reference: {0}", transactionReference);
                transaction.TransactionStatus = TransactionStatus.Failed;
                transaction.ResponseCode = $"{CREDIT_ERR_PREFIX}/{CEVA_NOTATION_PREFIX}/{walletFundingResponse.Error?.ResponseCode}";
                transaction.ResponseTime = DateTime.Now;
                await _transactionDAO.Update(transaction);

                response.FaultType = FaultMode.GATEWAY_ERROR;
                response.Error = new ErrorResponse()
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };

                return response;
            }

            if (!walletFundingResponse.IsSuccessful)
            {
                transaction.TransactionStatus = TransactionStatus.Failed;
                transaction.ResponseCode = walletFundingResponse.Error.ResponseCode;
                transaction.ResponseTime = DateTime.Now;
                await _transactionDAO.Update(transaction);
                await LogForReversal(transaction.ID, ReversalType.Wallet);
                response.Error = new ErrorResponse()
                {
                    ResponseCode = walletFundingResponse.Error.ResponseCode,
                    ResponseDescription = _messageProvider.GetMessage(walletFundingResponse.Error.ResponseCode, language)
                };

                return response;
            }
            response.IsSuccessful = true;
            return response;
        }

        private async Task<ServiceResponse<TransferResponse>> ProcessInterWalletTransaction(AuthenticatedTransferRequest request, Transaction transaction, string transactionReference, string language)
        {
            if (string.IsNullOrWhiteSpace(request.Narration))
            {
                request.Narration = null;
            }
            var response = new ServiceResponse<TransferResponse>(false);

            try
            {
                transaction.TransactionType = TransactionType.InterWalletTransfer;
                await _transactionDAO.Update(transaction);

                response = await _interWalletService.Transfer(request, transactionReference, language);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Interwallet transfer failure. Reference: {0}", transactionReference);
                transaction.TransactionStatus = TransactionStatus.InterWalletException;
                transaction.ResponseCode = $"{INTERWALLET_NOTATION_PREFIX}/{response.Error?.ResponseCode}";
                transaction.ResponseTime = DateTime.Now;
                await _transactionDAO.Update(transaction);

                response.FaultType = FaultMode.GATEWAY_ERROR;
                response.Error = new ErrorResponse()
                {
                    ResponseCode = ResponseCodes.TRANSACTION_FAILED,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.TRANSACTION_FAILED, language)
                };

                return response;
            }

            if (!response.IsSuccessful)
            {
                transaction.ResponseTime = DateTime.Now;
                transaction.TransactionStatus = TransactionStatus.Failed;
                transaction.ResponseCode = response.Error.ResponseCode;
                await _transactionDAO.Update(transaction);

                response.Error = new ErrorResponse()
                {
                    ResponseCode = ResponseCodes.TRANSACTION_FAILED,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.TRANSACTION_FAILED, language)
                };
                return response;
            }
            response.IsSuccessful = true;
            return response;
        }
    }
}