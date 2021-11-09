using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;

namespace Middleware.Service.Implementations
{
    public class PaymentBeneficiaryService : IPaymentBeneficiaryService
    {
        readonly IMessageProvider _messageProvider;
        readonly IBeneficiaryService _service;
        readonly IAuthenticator _authenticator;
        readonly ILogger _logger;
        private readonly IBenefitiariesDAO _benefitiariesDAO;
        private readonly ICodeGenerator _codeGenerator;
        public PaymentBeneficiaryService(IMessageProvider messageProvider, IBeneficiaryService service,
                                            IAuthenticator authenticator, ILoggerFactory logger, IBillsService billsService, IBenefitiariesDAO benefitiariesDAO, ICodeGenerator codeGenerator)
        {
            _messageProvider = messageProvider;
            _service = service;
            _authenticator = authenticator;
            _logger = logger.CreateLogger(typeof(PaymentBeneficiaryService));
            _benefitiariesDAO = benefitiariesDAO;
            _codeGenerator = codeGenerator;
        }

        public async Task<BasicResponse> AddBeneficiaryOld(NewPaymentBeneficiaryRequest request, string customerID, string language)
        {
            var response = new BasicResponse(false);
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
            response = await _authenticator.ValidateAnswer(customerID, request.Answer);
            if (!response.IsSuccessful)
            {
                response.FaultType = FaultMode.UNAUTHORIZED;
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                return response;
            }
            if (request.Beneficiary != null && !string.IsNullOrEmpty(request.Beneficiary.CustomerName))
            {
                request.Beneficiary.CustomerName = Util.EncodeString(request.Beneficiary.CustomerName);
            }
            if (request.Beneficiary != null && !string.IsNullOrEmpty(request.Beneficiary.Alias))
            {
                request.Beneficiary.Alias = Util.EncodeString(request.Beneficiary.Alias);
            }

            response = await _service.AddPaymentBeneficiary(request.Beneficiary, customerID);


            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
            }
            return response;
        }

        public async Task<BasicResponse> AddBeneficiary(NewPaymentBeneficiaryPaymentRequest request, AuthenticatedUser user, string language, string countryId)
        {
            _logger.LogInformation("Inside the AddBeneficiary method of the PaymentBeneficiaryService");
            var response = new BasicResponse(false);
            var beneficiary = request.Beneficiary;
            beneficiary.CountryId = countryId;
            var beneficiary_ = new AirTimeBenefitiary
            {
                PaymentType = request.Beneficiary.PaymentType,
                BillerCode = beneficiary.BillerCode,
                WalletNumber = user.WalletNumber,
                IsActive = true,
                IsDeleted = false,
                CustomerId = user.Id.ToString(),
                CountryId = countryId,
                Alias = beneficiary.Alias,
                ReferenceNumber = _codeGenerator.ReferralCode(25), 
                BeneficiaryName=beneficiary.BeneficiaryName, BeneficiaryNumber=beneficiary.BeneficiaryNumber
                , BillerName= beneficiary.BillerName
            };
            if (!beneficiary_.IsValid(out var source))
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
             
            response = await _authenticator.ValidateAnswer(beneficiary_.CustomerId, request.Answer);
            if (!response.IsSuccessful)
            {
                response.FaultType = FaultMode.UNAUTHORIZED;

                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                return response;
            }

 
            response = await _benefitiariesDAO.SaveAirtimeBeneficiary(beneficiary_, language);
             

            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
            }
            response.FaultType = FaultMode.NONE;
            return response;
        }



        public async Task<BasicResponse> DeleteBeneficiary(string beneficiaryID, string customerID, Answer answer, string language)
        {
            var response = await _authenticator.ValidateAnswer(customerID, answer);
            if (!response.IsSuccessful)
            {
                response.FaultType = FaultMode.UNAUTHORIZED;
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                return response;
            }
            response = await _service.RemovePaymentBeneficiary(beneficiaryID, customerID);
            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
            }
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<PaymentBeneficiary>>> GetBeneficiaries(string customerID, string language)
        {
            var result = await _service.GetPaymentBeneficiaries(customerID);
            if (!result.IsSuccessful)
            {
                result.Error.ResponseDescription = _messageProvider.GetMessage(result.Error.ResponseCode, language);
            }
            return result;
        }
    }
}
