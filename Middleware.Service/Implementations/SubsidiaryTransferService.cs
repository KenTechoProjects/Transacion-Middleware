using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Middleware.Service.Implementations
{
    public class SubsidiaryTransferService : ISubsidiaryTransferService
    {
        readonly ISubsidiaryService _service;
        readonly IMessageProvider _messageProvider;
        private readonly SystemSettings _settings;
        readonly IAuthenticator _authenticator;
        private readonly ILogger _log;
        public SubsidiaryTransferService(ISubsidiaryService Service, IMessageProvider messageProvider,
            IOptions<SystemSettings> settingsProvider, IAuthenticator authenticator, ILoggerFactory log)
        {
            _service = Service;
            _messageProvider = messageProvider;
            _settings = settingsProvider.Value;
            _authenticator = authenticator;
            _log = log.CreateLogger(typeof(SubsidiaryTransferService));
        }
        public async Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber, string subsidiaryID, string language)
        { var response = new ServiceResponse<dynamic>(false);
            try
            {
              
                _log.LogInformation("Inside the GetAcountName method of the SubsidiaryTransferService at {0} ", DateTime.UtcNow);
                _log.LogInformation("Calling the Subsidiary Service inside the SubsidiaryTransferService at {0}", DateTime.UtcNow);
                 response = await _service.GetAccountName(accountNumber, subsidiaryID);
                if (!response.IsSuccessful)
                {
                    response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                }
                return response;
            }
            catch(Exception ex)
            {
                _log.LogCritical(ex, "Server error Inside the GetAcountName method of the SubsidiaryTransferService at {0} ", DateTime.UtcNow);
                  response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                return response;
            }
          
        }

        public async Task<ServiceResponse<CrossBorderTransferCharge>> GetCharges(decimal amount, string language)
        {
            var response = await _service.GetCharges(amount);
            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
            }
            return response;
        }

        public async Task<ServiceResponse<ForexRate>> GetForexRate(string sourceCurrency, string targetCurrency, string language)
        {
            var response = await _service.GetForexRate(sourceCurrency, targetCurrency);
            if(!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
            }
            return response;
        }

        public async Task<IEnumerable<Subsidiary>> GetSubsidiaries()
        {
            return (await _service.GetSubsidiaries()).Where(c => c.CountryID != _settings.CountryId);
        }

        public async Task<ServiceResponse<TransferResponse>> Transfer(CrossBorderTransferRequest request, string customerID, string language, bool saveAsBeneficiary)
        {
            //validate limit
            //Create transaction    
            var response = new ServiceResponse<TransferResponse>(false);
            var validationResponse = await _authenticator.ValidatePin(customerID, request.Pin);
            if (!validationResponse.IsSuccessful)
            {
                response = new ServiceResponse<TransferResponse>(false)
                {
                    FaultType = FaultMode.UNAUTHORIZED,
                    Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_TRANSACTION_PIN,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_TRANSACTION_PIN, language)
                    }
                };
                return response;
            }
            var transactionReference = Guid.NewGuid().ToString();
            var rsp = await _service.Transfer(request, transactionReference);
            if(!rsp.IsSuccessful)
            {
                rsp.Error.ResponseDescription = _messageProvider.GetMessage(rsp.Error.ResponseCode, language);
                response.Error = rsp.Error;
                return response;
                //TODO: Set fault type
            }
            response.IsSuccessful = true;

            var transferResponse = new TransferResponse
            {
                Date = DateTime.Now.Date,
                Reference = transactionReference,
                BeneficiaryStatus = new BeneficiaryStatus
                {
                    Attempted = saveAsBeneficiary,
                },
                TransactionDetails = new TransactionDetails
                {
                    Amount = request.Amount,
                    DestinationAccountName = request.DestinationAccountName,
                    DestinationAccountID = request.DestinationAccountID,
                    Narration = request.Narration,
                    SourceAccountName = request.SourceAccountName,
                    SourceAccountNumber = request.SourceAccountNumber
                }
            };
            response.SetPayload(transferResponse);
            //TODO: save beneficiary if required
            return response;
        }
    }
}