using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Newtonsoft.Json;

namespace Middleware.Service.Beneficiary
{
    public class BeneficiaryService : IBeneficiaryService
    {
        private readonly BeneficiarySettings _beneficiaryConfigProvider;
        private readonly IMessageProvider _messageProvider;
        private readonly HttpClient _client;
        //string[] ACCOUNT_TYPE = { "WALLET", "BANK ACCOUNT" };
        //string[] PAYMENT_TYPE = { "AIRTIME", "BILLS" };
        const string APP_ID = "AppId";
        const string APP_KEY = "AppKey";
        const string customerIdHeader = "customerID";
        const string countryHeader = "countryId";
        private const string _prefix = "BMS";
        private const string DUPLICATE_BENEFICIARY = "BMS040";
        readonly ILogger _logger;
        private readonly IInstitutionDAO _institutionDAO;
        private readonly SystemSettings _settings;

        public BeneficiaryService(IOptions<BeneficiarySettings> beneficiaryConfigProvider, IHttpClientFactory clientFactory, ILoggerFactory logger, IInstitutionDAO institution,
                                    IMessageProvider messageProvider, IOptions<SystemSettings> setting)
        {
            _beneficiaryConfigProvider = beneficiaryConfigProvider.Value;
            _client = clientFactory.CreateClient("HttpMessageHandler");
            _client.BaseAddress = new Uri(_beneficiaryConfigProvider.BaseUrl);
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add(APP_ID, _beneficiaryConfigProvider.AppId);
            _client.DefaultRequestHeaders.Add(APP_KEY, _beneficiaryConfigProvider.AppKey);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _logger = logger.CreateLogger(typeof(BeneficiaryService));
            _institutionDAO = institution;
            _messageProvider = messageProvider;
            _settings = setting.Value;
        }

        public async Task<BasicResponse> AddTransferBeneficiary(TransferBeneficiary beneficiary, string username)
        {

            //var accountId = Util.GetonyNumbers(beneficiary.AccountId);
            //if (accountId.Length > 19)
            //{
            //    var accNumber = Util.RemoveCharacterFromBegining(accountId, 3);
            //    beneficiary.AccountId = accNumber;
            //}
            //else
            //{
            //    beneficiary.AccountId = accountId;
            //}
            var response = new BasicResponse(false);
            _logger.LogInformation("Inside the AddTransferBeneficiary");
            _client.DefaultRequestHeaders.Add(customerIdHeader, username);
            _client.DefaultRequestHeaders.Add(countryHeader, _settings.CountryId);
            _logger.LogInformation(" Customer id:{0}", username);

            string json = JsonConvert.SerializeObject(beneficiary);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _logger.LogInformation(" Sending   to api/beneficiaries/TransferBeneficiary endpoint inside the AddTransferBeneficiary :====>{0}", JsonConvert.SerializeObject(beneficiary));

            HttpResponseMessage responseMessage = await _client.PostAsync("api/beneficiaries/TransferBeneficiary", content);
            _logger.LogInformation(" Response  from api/beneficiaries/TransferBeneficiary endpoint inside the AddTransferBeneficiary====>{0}", JsonConvert.SerializeObject(responseMessage));
            switch (responseMessage.StatusCode)
            {
                case HttpStatusCode.Conflict:
                    _logger.LogInformation("There is conflict after calling api/beneficiaries/TransferBeneficiary endpoint inside the GetTransferBeneficiary");

                    if (response.Error.ResponseCode == "BMS040")
                    {

                        response.Error.ResponseCode = ResponseCodes.BENEFICIARY_ALREADY_EXISTS;
                    }
                    //else
                    //{
                    //    response.Error = new ErrorResponse
                    //    {
                    //        ResponseCode = DUPLICATE_BENEFICIARY
                    //    };
                    //}

                    return response;
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                    response.IsSuccessful = true;
                    return response;
                default:
                    throw new Exception("Service invocation failure");

            }
        }




        public async Task<ServiceResponse<IEnumerable<TransferBeneficiary>>> GetTransferBeneficiaries(string username)
        {
            _logger.LogInformation(" Inside GetTransferBeneficiaries of Beneficiary service");

            List<TransferBeneficiary> result = new List<TransferBeneficiary>();
            var response = new ServiceResponse<IEnumerable<TransferBeneficiary>>(false);
            if (string.IsNullOrWhiteSpace(username))
            {
                response.IsSuccessful = false;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, "en")
                };
                return response;
            }

            _client.DefaultRequestHeaders.Add(customerIdHeader, username);
            _client.DefaultRequestHeaders.Add(countryHeader, _settings.CountryId);
            // _client.DefaultRequestHeaders.Add("countryId", "04");

            _logger.LogInformation("CustomerId:{0}", username);
            HttpResponseMessage responseMessage = await _client.GetAsync("api/beneficiaries/TransferBeneficiary");
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception("Service invocation failure");
            }

            var rawResponse = await responseMessage.Content.ReadAsStringAsync();
            var deserializedBeneficiaries = JsonConvert.DeserializeObject<List<TransferBeneficiary>>(rawResponse);
            _logger.LogInformation(" Response from api/beneficiaries/TransferBeneficiary httpGet endpoint inside the AddTransferBeneficiary . Response====>{0}", JsonConvert.SerializeObject(deserializedBeneficiaries));

            if (deserializedBeneficiaries.Count() > 0)
            {
                var institutions = await _institutionDAO.GetAll();

                foreach (var beneficiary in deserializedBeneficiaries)
                {
                    result.Add(new TransferBeneficiary
                    {
                        Reference = beneficiary.Reference,
                        AccountName = Util.DecodeString(beneficiary.AccountName),
                        AccountId = beneficiary.AccountId,
                        InstitutionCode = beneficiary.InstitutionCode,
                        AccountType = beneficiary.AccountType,
                        Alias = Util.DecodeString(beneficiary.Alias),
                        InstitutionName = institutions.FirstOrDefault(x => x.InstitutionCode == beneficiary.InstitutionCode)?.InstitutionName
                    });
                }
                response.SetPayload(result.OrderByDescending(x => x.AccountName));
                response.IsSuccessful = true;
            }
            else
            {
                response.SetPayload(result);
                response.IsSuccessful = true;
            }
            _logger.LogInformation("Returned response from GettransferBeneficiary method of the BenfitiaryService  Response:===>{0}", JsonConvert.SerializeObject(response));

            return response;
        }

        public async Task<BasicResponse> RemoveTransferBeneficiary(string beneficiaryID, string username)
        {
            _logger.LogInformation("Inside the RemoveTransferBeneficiary of BeneficiaryService");
            var response = new BasicResponse(false);
            _client.DefaultRequestHeaders.Add(customerIdHeader, username);
            _client.DefaultRequestHeaders.Add(countryHeader, _settings.CountryId);
            _logger.LogInformation(" Sending to https://sharedproperties.azurewebsites.net/api/beneficiaries/TransferBeneficiary HttpDelete endpoint inside the AddTransferBeneficiary", beneficiaryID);

            HttpResponseMessage responseMessage = await _client.DeleteAsync($"api/beneficiaries/TransferBeneficiary/{beneficiaryID}");
            _logger.LogInformation(" Response from https://sharedproperties.azurewebsites.net/api/beneficiaries/TransferBeneficiary HttpDelete endpoint inside the AddTransferBeneficiary. reponsee=====> {0}", JsonConvert.SerializeObject(responseMessage));

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception("Service invocation failure");
            }

            //  var rawResponse = await responseMessage.Content.ReadAsStringAsync();
            //  var responseCode = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawResponse);
            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {
                response.IsSuccessful = true;
            }
            else
            {
                response.Error.ResponseCode = ResponseCodes.GENERAL_ERROR;
                response.Error.ResponseDescription = "Unsuccessful request";
            }
            _logger.LogInformation("Returned response from RemoveTransferBeneficiary method of the BenfitiaryService  RResponse:===>{0}", JsonConvert.SerializeObject(response));

            return response;
        }

        public async Task<ServiceResponse<IEnumerable<PaymentBeneficiary>>> GetPaymentBeneficiaries(string username)
        {
            _logger.LogInformation("Inside the GetPaymentBeneficiaries of BeneficiaryServive");
            var response = new ServiceResponse<IEnumerable<PaymentBeneficiary>>(false);
            _client.DefaultRequestHeaders.Add(customerIdHeader, username);
            _client.DefaultRequestHeaders.Add(countryHeader, _settings.CountryId);
            _logger.LogInformation("GET_PAYMENT_BENEFICIARY_REQUEST USERNAME: {0}", username);
            _logger.LogInformation("Calling httpGet of api/PaymentBeneficiary   Inside the GetPaymentBeneficiaries of BeneficiaryServive");
            HttpResponseMessage responseMessage = await _client.GetAsync("api/PaymentBeneficiary");
            _logger.LogInformation("Response from calling httpGet of  api/PaymentBeneficiary   Inside the GetPaymentBeneficiaries of BeneficiaryServive {0}", JsonConvert.SerializeObject(responseMessage));


            _logger.LogInformation("GET_PAYMENT_BENEFICIARY_RESPONSE: {0}", JsonConvert.SerializeObject(responseMessage));
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception("Service invocation failure");
            }

            var rawResponse = await responseMessage.Content.ReadAsStringAsync();
            _logger.LogInformation("GET PAYMENT BENEFICIARY rawResponse:{0}", rawResponse);
            var result = Util.DeserializeFromJson<List<PaymentBeneficiary>>(rawResponse);
            foreach (var item in result)
            {
                item.CustomerName = Util.DecodeString(item.CustomerName);
                item.Alias = Util.DecodeString(item.Alias);
            }
            response.SetPayload(result.OrderByDescending(x => x.CustomerName));
            response.IsSuccessful = true;
            _logger.LogInformation("Returned response from GetPaymentBeneficiary method of the BenfitiaryService  RResponse:===>{0}", JsonConvert.SerializeObject(response));

            return response;
        }

        public async Task<BasicResponse> AddPaymentBeneficiary(PaymentBeneficiary beneficiary, string username)
        {
            var response = new BasicResponse(false);
            _client.DefaultRequestHeaders.Add(customerIdHeader, username);
            _client.DefaultRequestHeaders.Add(countryHeader, _settings.CountryId);
            string json = JsonConvert.SerializeObject(beneficiary);
            _logger.LogInformation("PAYMENT_BENEFICIARY_REQUEST: {0}, USERNAME: {1}", json, username);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = await _client.PostAsync("api/PaymentBeneficiary", content);

            _logger.LogInformation("PAYMENT_BENEFICIARY_RESPONSE: {0}", JsonConvert.SerializeObject(responseMessage));

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception("Service invocation failure");
            }

            var rawResponse = await responseMessage.Content.ReadAsStringAsync();

            _logger.LogInformation("ADD PAYMENT BENEFICIARY rawResponse: {0}", JsonConvert.SerializeObject(rawResponse));

            if (responseMessage.IsSuccessStatusCode)
            {
                response.IsSuccessful = true;
            }
            _logger.LogInformation("Returned response from AddPaymentBeneficiary method of the BenfitiaryService  Response:===>{0}", JsonConvert.SerializeObject(response));
            return response;
        }

        public async Task<BasicResponse> RemovePaymentBeneficiary(string beneficiaryID, string username)
        {
            var response = new BasicResponse(false);
            _client.DefaultRequestHeaders.Add(customerIdHeader, username);
            _client.DefaultRequestHeaders.Add(countryHeader, _settings.CountryId);
            HttpResponseMessage responseMessage = await _client.DeleteAsync($"api/PaymentBeneficiary/{beneficiaryID}");

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception("Service invocation failure");
            }

            var rawResponse = await responseMessage.Content.ReadAsStringAsync();
            var responseCode = JsonConvert.DeserializeObject<Dictionary<string, string>>(rawResponse);
            _logger.LogInformation("response from api/PaymentBeneficiary/beneficiaryID Response:===>{0}", JsonConvert.SerializeObject(responseCode));
            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {
                response.IsSuccessful = true;
            }
            else
            {
                response.Error.ResponseCode = ResponseCodes.GENERAL_ERROR;
                response.Error.ResponseDescription =  "Unsuccessful request";
            }
            _logger.LogInformation("Returned response from RemovePaymentBeneficiary method of the BenfitiaryService  RResponse:===>{0}", JsonConvert.SerializeObject(response));

            return response;
        }
    }
}
