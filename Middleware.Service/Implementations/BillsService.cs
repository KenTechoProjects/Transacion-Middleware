
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Middleware.Core.DAO;
using Middleware.Core.DTO;
using Middleware.Core.Model;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.Model;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Newtonsoft.Json;
using Middleware.Service.BAP;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace Middleware.Service.Implementations
{
    public class BillsService : IBillsService
    {
        private readonly IBillerDAO _billerDAO;
        private readonly IProductDAO _productDAO;
        private readonly ILogger _logger;
        private readonly IMessageProvider _messageProvider;
        private readonly IPaymentManager _paymentManager;
        private readonly IAuthenticator _authenticator;
        private readonly ILimitService _limitService;
        private readonly ITransactionDAO _transactionDAO;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly IBenefitiariesDAO _airtimeDAO;
        private readonly BillsPaySettings _provider;
        private readonly ICodeGenerator  _codeGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;
    
        public BillsService(
            IBillerDAO billerDAO,
            IProductDAO productDAO,
            ILoggerFactory logger,
            IMessageProvider messageProvider,
            IBeneficiaryService beneficiaryService,
            IPaymentManager paymentManager,
            IAuthenticator authenticator,
            ILimitService limitService,
            ITransactionDAO transactionDAO,
            IOptions<BillsPaySettings> provider
, IBenefitiariesDAO airtimeDAO, IHttpContextAccessor httpContextAccessor, ICodeGenerator codeGenerator)
        {
            _billerDAO = billerDAO;
            _productDAO = productDAO;
            _logger = logger.CreateLogger(typeof(BillsService));
            _messageProvider = messageProvider;
            _paymentManager = paymentManager;
            _authenticator = authenticator;
            _limitService = limitService;
            _transactionDAO = transactionDAO;
            _beneficiaryService = beneficiaryService;
            _provider = provider.Value;
            _airtimeDAO = airtimeDAO;
            _httpContextAccessor = httpContextAccessor;
            _codeGenerator = codeGenerator;
        }

        public async Task<ServiceResponse<PaymentResponse>> GetChannelBillers()
        {
            var response = new ServiceResponse<PaymentResponse>(true);
            var rsp = await _paymentManager.GetChannelBillers();
            return response;
        }

        public async Task<ServiceResponse<PaymentResponse>> GetBapBillers()
        {
            var response = new ServiceResponse<PaymentResponse>(true);
            var rsp = await _paymentManager.GetBapBillers();
            return response;
        }

        public async Task<ServiceResponse<PaymentResponse>> GetBapProducts(string slug)
        {
            var response = new ServiceResponse<PaymentResponse>(true);
            var rsp = await _paymentManager.GetBapProducts(slug);
            return response;
        }

        public async Task<ServiceResponse<PaymentResponse>> GetBapIProducts(string slug)
        {
            var response = new ServiceResponse<PaymentResponse>(true);
            var rsp = await _paymentManager.GetBapIProducts(slug);
            return response;
        }

        public async Task<ServiceResponse<PaymentResponse>> GetAirtimeBillers()
        {
            var response = new ServiceResponse<PaymentResponse>(true);
            var rsp = await _paymentManager.GetAirtimeBillers();
            return response;
        }

        public async Task<ServiceResponse<PaymentResponse>> Wildcard()
        {
            var response = new ServiceResponse<PaymentResponse>(true);
            var rsp = await _paymentManager.Wildcard();
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<BillerInfo>>> GetBillers(string language)
        {
            var billers = await _billerDAO.GetActiveBillers(BillerType.BILL);
            if (!billers.Any())
            {
                return ErrorResponse.Create<ServiceResponse<IEnumerable<BillerInfo>>>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                    ResponseCodes.BILLERS_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.BILLERS_NOT_FOUND, language));
            }
            var payload = billers.Select(s => new BillerInfo
            {
                BillerCode = s.BillerCode,
                BillerName = s.BillerName

            }).ToArray();

            var response = new ServiceResponse<IEnumerable<BillerInfo>>(true);
            response.SetPayload(payload);
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<ProductInfo>>> GetProducts(string billerCode, string language)
        {
            var products = await _productDAO.GetActiveProducts(billerCode);
            if (!products.Any())
            {
                return ErrorResponse.Create<ServiceResponse<IEnumerable<ProductInfo>>>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                    ResponseCodes.PRODUCTS_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.PRODUCTS_NOT_FOUND, language));
            }
            var payload = products.Select(p => new ProductInfo
            {
                IsFixedAmount = p.IsFixedAmount,
                Price = p.Price,
                Surcharge = _provider.Surcharge,
                Vat = _provider.Vat,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                ReferenceName = p.ReferenceName,
                ValidationSupported = p.ValidationSupported,
                RequestParams = string.IsNullOrEmpty(p.AdditionalParameters) ? null :
                                    JsonConvert.DeserializeObject<Dictionary<string, string>>(p.AdditionalParameters)

            }).ToArray();

            var response = new ServiceResponse<IEnumerable<ProductInfo>>(true);
            response.SetPayload(payload);
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<BillerInfo>>> GetTelcos(string language)
        {
            var billers = await _billerDAO.GetActiveBillers(BillerType.TELCO);
            if (!billers.Any())
            {
                return ErrorResponse.Create<ServiceResponse<IEnumerable<BillerInfo>>>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                    ResponseCodes.BILLERS_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.BILLERS_NOT_FOUND, language));
            }
            var payload = billers.Select(s => new BillerInfo
            {
                BillerCode = s.BillerCode,
                BillerName = s.BillerName

            }).ToList();
            var response = new ServiceResponse<IEnumerable<BillerInfo>>(true);
            response.SetPayload(payload);
            return response;
            //var response = await _paymentManager.GetTelcos();
            //if (!response.IsSuccessful)
            //{
            //    response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
            //}
            //return response;
        }


        private async Task<BasicResponse> SaveBeneficiary(BuyAirtimeRequest request, string customerId, DTOs.PaymentType type)
        {
            var response = new BasicResponse(false);

            var beneficiary = new PaymentBeneficiary
            {
                BillerCode = request.TelcoCode,
                BillerName = request.TelcoCode,
                ReferenceNumber = request.PhoneNumber,
                PaymentType = type
            };

            response = await _beneficiaryService.AddPaymentBeneficiary(beneficiary, customerId);

            return response;
        }

        public async Task<ServiceResponse<PaymentResponse>> PayBill(BillPaymentRequest request, AuthenticatedUser user, bool saveBeneficiary, string language)
        {
            var response = new ServiceResponse<PaymentResponse>(false);
            var validationResponse = await _authenticator.ValidatePin(user.UserName, request.Pin);
            if (!validationResponse.IsSuccessful)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = validationResponse.Error.ResponseCode,
                    ResponseDescription = _messageProvider.GetMessage(validationResponse.Error.ResponseCode, language)

                };
                return response;
            }

            var isWithinLimit = await _limitService.ValidateLimit(user.Id, TransactionType.BillPayment, request.Amount);
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
                response = new ServiceResponse<PaymentResponse>(false)
                {
                    FaultType = FaultMode.INVALID_OBJECT_STATE,
                    Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.LIMIT_EXCEEDED,
                        ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEEDED, language)}"
                    }

                };
                return response;
            }


            //optionally save beneficiary
            var product = await _productDAO.FindProduct(request.ProductCode);
            if (product == null)
            {
                return ErrorResponse.Create<ServiceResponse<PaymentResponse>>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                    ResponseCodes.PRODUCT_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.PRODUCT_NOT_FOUND, language));
            }
            IDictionary<string, string> productParams = null;
            if (!string.IsNullOrEmpty(product.GatewayMetadata))
            {
                productParams = JsonConvert.DeserializeObject<IDictionary<string, string>>(product.GatewayMetadata);
            }

            var reference = _paymentManager.GenerateReference();
            var transaction = new Transaction
            {
                CustomerId = user.Id,
                Amount = request.Amount,
                BillerID = request.BillerCode,
                SourceAccountId = request.SourceAccountId,
                //ProviderReference = request.CustomerReference,
                TransactionType = TransactionType.BillPayment,
                TransactionStatus = TransactionStatus.New,
                TransactionReference = reference,
                DateCreated = DateTime.UtcNow
            };
            await _transactionDAO.Add(transaction);
            try
            {
                var rsp = await _paymentManager.PayBill(request, reference, user.WalletNumber, productParams);
                transaction.ResponseTime = DateTime.UtcNow;
                var transactionStatus = Util.GetTransactionStatus(rsp.Status);
                transaction.TransactionStatus = transactionStatus;
                if (rsp.IsSuccessful)
                {
                    var payload = new PaymentResponse
                    {
                        Date = DateTime.UtcNow,
                        Reference = reference,
                        Status = transactionStatus,
                        Message = transactionStatus == TransactionStatus.Pending ? "Payment is being processed" : "Payment Successful"
                    };
                    response.IsSuccessful = true;
                    response.SetPayload(payload);
                    if (saveBeneficiary)
                    {
                        var beneficiaryStatus = await SaveBeneficiary(request, PaymentType.BILL, user);
                        payload.BeneficiaryStatus = new BeneficiaryStatus
                        {
                            Attempted = true,
                            IsSuccessful = beneficiaryStatus.IsSuccessful,
                            Message = beneficiaryStatus.IsSuccessful==true ? _messageProvider.GetMessage(ResponseCodes.BENEFICIARY_SAVED_SUCCESSFULLY,language) : _messageProvider.GetMessage(ResponseCodes.UNABLE_TO_SAVE_BENEFICIARY, language) 
                        };
                    }

                }
                else
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = rsp.Error.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(rsp.Error.ResponseCode, language)
                    };
                }
                await _transactionDAO.Update(transaction);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Payment error for {0}", user.UserName);
                return ErrorResponse.Create<ServiceResponse<PaymentResponse>>(FaultMode.SERVER, ResponseCodes.GENERAL_ERROR,
                    _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language));
            }


        }

        public async Task<ServiceResponse<PaymentValidationPayload>> Validate(PaymentValidationRequest request, string language)
        {
            var product = await _productDAO.FindProduct(request.ProductCode);
            if (product == null)
            {
                return ErrorResponse.Create<ServiceResponse<PaymentValidationPayload>>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                    ResponseCodes.PRODUCT_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.PRODUCT_NOT_FOUND, language));
            }
            IDictionary<string, string> productParams = null;
            if (!string.IsNullOrEmpty(product.GatewayMetadata))
            {
                productParams = JsonConvert.DeserializeObject<IDictionary<string, string>>(product.GatewayMetadata);
            }
            var response = await _paymentManager.Validate(request, productParams);
            if (!response.IsSuccessful)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = response.Error.ResponseCode,
                    ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language)
                };
            }
            return response;
        }

        public async Task<ServiceResponse<PaymentResponse>> BuyAirtime(AirtimePurchaseRequest request, AuthenticatedUser user, bool saveBeneficiary, string language)
        {
            var response = new ServiceResponse<PaymentResponse>(false);
            var validationResponse = await _authenticator.ValidatePin(user.UserName, request.Pin);
            if (!validationResponse.IsSuccessful)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = validationResponse.Error.ResponseCode,
                    ResponseDescription = _messageProvider.GetMessage(validationResponse.Error.ResponseCode, language)

                };
                return response;
            }

            var isWithinLimit = await _limitService.ValidateLimit(user.Id, TransactionType.BillPayment, request.Amount);
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
                response = new ServiceResponse<PaymentResponse>(false)
                {
                    FaultType = FaultMode.INVALID_OBJECT_STATE,
                    Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.LIMIT_EXCEEDED,
                        ResponseDescription = $"{isWithinLimit.LimitType} {_messageProvider.GetMessage(ResponseCodes.LIMIT_EXCEEDED, language)}"
                    }

                };
                return response;
            }

            var product = await _productDAO.GetAnyProduct(request.TelcoCode);
            if (product == null)
            {
                return ErrorResponse.Create<ServiceResponse<PaymentResponse>>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                    ResponseCodes.PRODUCT_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.PRODUCT_NOT_FOUND, language));
            }
            var reference = _paymentManager.GenerateReference();
            var transaction = new Transaction
            {
                CustomerId = user.Id,
                Amount = request.Amount,
                BillerID = request.TelcoCode,
                SourceAccountId = request.SourceAccountId,
                TransactionType = TransactionType.Airtime,
                TransactionStatus = TransactionStatus.New,
                TransactionReference = reference,
                DateCreated = DateTime.Now
            };
            await _transactionDAO.Add(transaction);
            var purchaseRequest = new BasePaymentRequest
            {
                Amount = request.Amount,
                BillerCode = request.TelcoCode,
                ProductCode = product.ProductCode,
                CustomerReference = request.PhoneNumber,
                SourceAccountId = request.SourceAccountId,
                SourceAccountType = request.SourceAccountType
            };

            try
            {
                var rsp = await _paymentManager.PayBill(purchaseRequest, reference, user.WalletNumber, null);

                transaction.ResponseTime = DateTime.UtcNow;
                transaction.TransactionStatus = rsp.IsSuccessful ? TransactionStatus.Successful : TransactionStatus.Failed; //TODO: Handle pending transactions

                await _transactionDAO.Update(transaction);

                if (rsp.IsSuccessful)
                {
                    var payload = new PaymentResponse
                    {
                        Date = DateTime.UtcNow,
                        Reference = reference
                    };
                    response.IsSuccessful = true;
                    response.SetPayload(payload);
                    if (saveBeneficiary)
                    {
                        var beneficiaryStatus = await SaveBeneficiary(purchaseRequest, PaymentType.AIRTIME, user);
                        payload.BeneficiaryStatus = new BeneficiaryStatus
                        {
                            Attempted = true,
                            IsSuccessful = beneficiaryStatus.IsSuccessful,
                            Message = beneficiaryStatus.IsSuccessful==true ? _messageProvider.GetMessage(ResponseCodes.BENEFICIARY_SAVED_SUCCESSFULLY, language) : _messageProvider.GetMessage(ResponseCodes.UNABLE_TO_SAVE_BENEFICIARY, language)
                        };

                    }
                }
                else
                {

                    response.Error = new ErrorResponse
                    {
                        ResponseCode = rsp.Error.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(rsp.Error.ResponseCode, language)
                    };
                }


                return response;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("A connection attempt failed"))
                {
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                        ResponseDescription =
                            _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                    };
                    return response;
                }
                _logger.LogCritical(e, "Airtime purchase error for {0}", user.UserName);
                return ErrorResponse.Create<ServiceResponse<PaymentResponse>>(FaultMode.SERVER, ResponseCodes.GENERAL_ERROR,
                    _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language));
            }
        }

        private async Task<BasicResponse> SaveBeneficiary(BasePaymentRequest request, PaymentType type, AuthenticatedUser user)
        {
            var response = new BasicResponse(false);
            var beneficiary = new PaymentBeneficiary
            {
                BillerCode = request.BillerCode,
                PaymentType = type,
                ReferenceNumber = request.CustomerReference
            };
            try
            {
                response = await _beneficiaryService.AddPaymentBeneficiary(beneficiary, user.WalletNumber);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("A connection attempt failed"))
                {
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                        ResponseDescription =
                            _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, "en")
                    };
                    return response;
                }
                _logger.LogCritical(e, "Could not save beneficiary after payment. biller code: {0} - customerID: {1}",
                      request.BillerCode, request.CustomerReference);
            }

            return response;
        }

        public async Task<BasicResponse> SaveAirtimeBeneficiary(AirTimeBenefitiary beneficiary, PaymentType type, string walletNumber, string language, string countryId)
        {
            var response = new BasicResponse(false);



            try
            {       var custId = _httpContextAccessor.HttpContext.Request.Headers["customerId"];
                beneficiary.PaymentType = type;
                beneficiary.WalletNumber = walletNumber;
                beneficiary.IsActive = true;
                beneficiary.IsDeleted = false;
                beneficiary.CustomerId =  custId;
                //beneficiary.Alias = alias;
                beneficiary.CountryId = countryId;
                if (beneficiary.IsValid(out string surce) == false)
                {
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = ResponseCodes.REQUEST_NOT_COMPLETED,
                        ResponseDescription = $" { _messageProvider.GetMessage(ResponseCodes.INPUT_VALIDATION_FAILURE, language)}: Source ={surce}"

                    };
                }
           

                // var alias = _httpContextAccessor.HttpContext.Request.Headers["alias"];

                beneficiary.ReferenceNumber = _codeGenerator.ReferralCode(25);
                response = await _airtimeDAO.SaveAirtimeBeneficiary(beneficiary, language);


            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Could not save beneficiary for airtime. Request: {0}", JsonConvert.SerializeObject(beneficiary));
                response.Error = new ErrorResponse()
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription =
                          _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };
                return response;


            }

            return response;
        }

 

        public async Task<ServiceResponse> GetPaymentBeneficiariesAsyc(string walletNumber, string countryId, string language)
        {
            _logger.LogInformation( "Inside the GetPaymentBeneficiariesAsyc method of BillService");
            var response = new ServiceResponse(false);
            try
            {
                var result = await _airtimeDAO.GetPaymentBeneficiariesAsyc(walletNumber, countryId);
                response.IsSuccessful = true;
                response.Data = result;
                response.FaultType = FaultMode.NONE;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error ocuurred in the GetPaymentBeneficiariesAsyc method of BillService");
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };
                response.FaultType = FaultMode.SERVER;
            }

            return response;

        }

        public async Task<BasicResponse> DeletePaymentBeneficiariesAsyc(RemovePaymentBeneficiaryPaymentRequest request,AuthenticatedUser user, string language)
        {

            _logger.LogInformation("Inside the DeletePaymentBeneficiariesAsyc method of BillService");
            var response = new BasicResponse(false);
            try
            {

                 response = await _authenticator.ValidateAnswer(user.Id.ToString(), request.Answer);
                if (!response.IsSuccessful)
                {
                    response.FaultType = FaultMode.UNAUTHORIZED;

                    response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                    return response;
                }
                response = await _airtimeDAO.DeletePaymentBeneficiariesAsyc(request.BeneficiaryId,user.WalletNumber,user.Id.ToString()   , language);
                
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error ocuurred in the DeletePaymentBeneficiariesAsyc method of BillService");
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR, 
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };
                response.FaultType = FaultMode.SERVER;
            }

            return response;
       
        }
    }
}
