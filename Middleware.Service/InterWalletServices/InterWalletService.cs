using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Newtonsoft.Json;

namespace Middleware.Service.InterWalletServices
{
    public class InterWalletService : IInterWalletService
    {
        private readonly InterWalletTransferSettings _interWalletSettings;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        const string APP_ID = "AppId";
        const string APP_KEY = "AppKey";
        const string MEDIA_TYPE = "application/json";
        const string SUCCESS_RESPONSE_CODE = "00";
        const string _prefix = "IW";
        const string INSUFFICIENT_RESPONSE_CODE = "09";

        public InterWalletService(IOptions<InterWalletTransferSettings> interWalletSettings, IHttpClientFactory clientFactory, 
            ILoggerFactory logger)
        {
            _interWalletSettings = interWalletSettings.Value;
            _httpClient = clientFactory.CreateClient("HttpMessageHandler");
            _httpClient.BaseAddress = new Uri(_interWalletSettings.BaseAddress);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(APP_ID, _interWalletSettings.AppId);
            _httpClient.DefaultRequestHeaders.Add(APP_KEY, _interWalletSettings.AppKey);
            _httpClient.Timeout = TimeSpan.FromSeconds(_interWalletSettings.RequestTimeout);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MEDIA_TYPE));
            _logger = logger.CreateLogger(typeof(InterWalletService));
        }

        private string GetGeneratedRequestId()
        {
            return Guid.NewGuid().ToString();
        }


        public async Task<ServiceResponse<IEnumerable<WalletSchemes>>> GetSchemes()
        {
            var response = new ServiceResponse<IEnumerable<WalletSchemes>>(false);

            var request = new
            {
                RequestId = GetGeneratedRequestId(),
                _interWalletSettings.CountryId
            };
            string jsonRequest = JsonConvert.SerializeObject(request);
            _logger.LogInformation("GET_SCHEMES_REQUEST: {0}", jsonRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, MEDIA_TYPE);
            HttpResponseMessage rspMessage = await _httpClient.PostAsync(_interWalletSettings.EndPoints.WalletSchemes, content);
            _logger.LogInformation("GET_SCHEME_RESPONSE_MESSAGE: {0}", JsonConvert.SerializeObject(rspMessage));

            if (!rspMessage.IsSuccessStatusCode)
            {
                throw new Exception("Service invocation failure");
            }

            var rawResponse = await rspMessage.Content.ReadAsStringAsync();
            _logger.LogInformation("GET_SCHEME_RAW_RESPONSE: {0}",JsonConvert.SerializeObject(rawResponse));
            var schemes = JsonConvert.DeserializeObject<InterWalletSchemesResponse>(rawResponse);

            if (schemes.WalletSchemes.Count() > 0)
            {
                response.SetPayload(schemes.WalletSchemes);
                response.IsSuccessful = true;
            }
            else
            {
                response.SetPayload(new InterWalletSchemesResponse().WalletSchemes);
                response.IsSuccessful = true;
            }

            return response;
        }

        public async Task<ServiceResponse<string>> GetWalletName(string walletId, string walletScheme)
        {
            var response = new ServiceResponse<string>(false);

            var request = new
            {
                RequestId = GetGeneratedRequestId(),
                _interWalletSettings.CountryId,
                WalletId = walletId,
                WalletSchemeCode = walletScheme
            };
            string jsonRequest = JsonConvert.SerializeObject(request);
            _logger.LogInformation("GET_WALLET_NAME_REQUEST: {0}", jsonRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, MEDIA_TYPE);
            HttpResponseMessage rspMessage = await _httpClient.PostAsync(_interWalletSettings.EndPoints.NameEnquiry, content);

            if (!rspMessage.IsSuccessStatusCode)
            {
                throw new Exception("Service invocation failure");
            }
            var rawResponse = await rspMessage.Content.ReadAsStringAsync();
            _logger.LogInformation("GET_WALLET_NAME_RESPONSE: {0}",JsonConvert.SerializeObject(rawResponse));
            var walletNameResponse = JsonConvert.DeserializeObject<InterWalletNameEnquiryResponse>(rawResponse);

            if (walletNameResponse.ResponseCode == SUCCESS_RESPONSE_CODE)
            {
                response.SetPayload(walletNameResponse.WalletName);
                response.IsSuccessful = true;
            }
            else
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = $"{_prefix}{walletNameResponse.ResponseCode}" 
                };
                return response;
            }
            return response;
        }

        public async Task<BasicResponse> GetTransactionStatus(string transactionReference)
        {
            var response = new BasicResponse(false);

            var request = new
            {
                RequestId = GetGeneratedRequestId(),
                _interWalletSettings.CountryId,
                ClientReferenceId = transactionReference
            };
            string jsonRequest = JsonConvert.SerializeObject(request);
            _logger.LogInformation("GET_GET_TRANSACTION_STATUS_REQUEST: {0}", jsonRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, MEDIA_TYPE);
            HttpResponseMessage rspMessage = await _httpClient.PostAsync(_interWalletSettings.EndPoints.TransactionStatus, content);

            if (!rspMessage.IsSuccessStatusCode)
            {
                throw new Exception("Service invocation failure");
            }
            var rawResponse = await rspMessage.Content.ReadAsStringAsync();
            _logger.LogInformation("GET_TRANSACTION_STATUS_RESPONSE: {0}",JsonConvert.SerializeObject( rawResponse));
            var transStatusResponse = JsonConvert.DeserializeObject<InterWalletTransStatusResponse>(rawResponse);

            if (transStatusResponse.ResponseCode == SUCCESS_RESPONSE_CODE)
            {
                response.IsSuccessful = true;
            }
            else
            {
                response.Error.ResponseCode = transStatusResponse.ResponseCode;
                response.Error.ResponseDescription = "Transaction Failed";
            }

            return response;
        }

        public async Task<ServiceResponse<TransferResponse>> Transfer(AuthenticatedTransferRequest request, string transactionReference, string language)
        {
            var response = new ServiceResponse<TransferResponse>(false);

            var transferRequest = new
            {
                RequestId = GetGeneratedRequestId(),
                 _interWalletSettings.CountryId,
                SourceSchemeCode = _interWalletSettings.FBNWalletSchemeCode,
                SourceWalletId = request.SourceAccountId,
                Amount = (_interWalletSettings.InterwalletFeePercentage/100 * request.Amount) + request.Amount,
                request.Narration,
                DestinationSchemeCode = request.DestinationInstitutionId.Replace("-", "//"),
                DestinationWalletId = request.DestinationAccountId,
                ClientReferenceId = transactionReference,
                ClientSession = Guid.NewGuid().ToString()
            };
            var jsonRequest = JsonConvert.SerializeObject(transferRequest);
            _logger.LogInformation("INTER_WALLET_TRANSFER_REQUEST: {0}", jsonRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, MEDIA_TYPE);
            HttpResponseMessage rspMessage = await _httpClient.PostAsync(_interWalletSettings.EndPoints.Transfer, content);

            _logger.LogInformation("RESPONSE_MESSAGE: {0}", JsonConvert.SerializeObject(rspMessage));
            if (!rspMessage.IsSuccessStatusCode)
            {
                throw new Exception("Service invocation failure");
            }

            var rawResponse = await rspMessage.Content.ReadAsStringAsync();
            _logger.LogInformation("INTER_WALLET_TRANSFER_RESPONSE: {0}",JsonConvert.SerializeObject(rawResponse));
            var transferResponse = JsonConvert.DeserializeObject<TransferResponse>(rawResponse);
           
            if (transferResponse.ResponseCode != SUCCESS_RESPONSE_CODE)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = $"{_prefix}{transferResponse.ResponseCode}" //get possible response codes from Funsho to add to middleware response code library
                };
                return response;
            }
            
            response.IsSuccessful = true;
            response.SetPayload(transferResponse);
            return response;
        }


        public async Task<ServiceResponse<IEnumerable<WalletSchemes>>> GetWalletSchemes()
        {
            var walletScheme = new List<WalletSchemes>(new[]
            {
                new WalletSchemes
                {
                    Code = "PROVIDER-WALLET_BANK-QIWI",
                    Name = "Ecobank Agency Banking"
                },
                new WalletSchemes
                {
                    Code = "PROVIDER-WALLET-WARI",
                    Name = "BRN"
                }

            });
            var response = new ServiceResponse<IEnumerable<WalletSchemes>>(true);
            response.SetPayload(walletScheme);
            return await Task.FromResult(response);
        }
        

        public async Task<ServiceResponse<string>> DoWalletNameValidation(string walletId, string walletScheme)
        {
            var response = new ServiceResponse<string>(true);

            if (walletScheme == "PROVIDER-WALLET_BANK-QIWI" && walletId == "221771574721")
            {
                response.SetPayload("Olufunso Olunaike");
            }
            else if (walletScheme == "PROVIDER-WALLET-WARI" && walletId == "221771574721")
            {
                response.SetPayload("Ayodeji Babatunde");
            }
            else
            {
                response.SetPayload("Invalid Wallet Scheme");
                response.IsSuccessful = false;
            }

            return await Task.FromResult(response);
        }

        public async Task<ServiceResponse<ServiceChargeResponse>> GetServiceCharge(decimal amount, decimal balance)
        {
            var response = new ServiceResponse<ServiceChargeResponse>(false);

            var txtChargeAmount = (_interWalletSettings.InterwalletFeePercentage / 100 * amount);

            if ((amount + txtChargeAmount) > balance)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INSUFFICIENT_RESPONSE_CODE
                };
                return await Task.FromResult(response);
            }

            var serviceCharge = new ServiceChargeResponse
            {
                ChargeAmount = txtChargeAmount
            };

            response.SetPayload(serviceCharge);
            response.IsSuccessful = true;

            return await Task.FromResult(response);
        }
        
    }
}
