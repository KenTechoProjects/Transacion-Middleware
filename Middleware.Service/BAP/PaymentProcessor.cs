using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.BAP
{
    public class PaymentProcessor : IPaymentProcessor
    {
        readonly HttpClient _client;
        readonly BillsPaySettings _options;
        readonly ILogger _logger;
        static string MEDIA_TYPE = "application/json";
        static string KEY = "Api-Key";

        public PaymentProcessor(IHttpClientFactory clientFactory, IOptions<BillsPaySettings> options, ILoggerFactory logger)
        {
            _options = options.Value;
            _client = clientFactory.CreateClient("HttpMessageHandler");
            _client.BaseAddress = new Uri(_options.BaseAddress);
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add(KEY, _options.AppKey);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MEDIA_TYPE));
            _logger = logger.CreateLogger(typeof(PaymentProcessor));
        }

        public async Task<ServiceResponse<BillsPayResponse>> MakePaymentByWallet(WalletBillsPayRequest request)
        {
            var serviceResponse = new ServiceResponse<BillsPayResponse>(false);
            string serializedRequest = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, MEDIA_TYPE);
            _logger.LogInformation($"WalletRequest: ",serializedRequest);
            HttpResponseMessage respMsg = await _client.PostAsync(_options.TransactionPath, requestContent);

            if (!respMsg.IsSuccessStatusCode)
            {
                return serviceResponse;
            }

            var response = await respMsg.Content.ReadAsStringAsync();
            _logger.LogInformation($"RESPONSE:", response);
            var deserializedObject = JsonConvert.DeserializeObject<BillsPayResponse>(response);

            if (deserializedObject.Status)
            {
                serviceResponse.IsSuccessful = true;
                serviceResponse.SetPayload(deserializedObject);
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<BillsPayResponse>> MakePaymentByAccount(AccountBillsPayRequest request)
        {
            var serviceResponse = new ServiceResponse<BillsPayResponse>(false);
            string serializedRequest = JsonConvert.SerializeObject(request);
            _logger.LogInformation($"REQUEST:", serializedRequest);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, MEDIA_TYPE);
            HttpResponseMessage respMsg = await _client.PostAsync(_options.TransactionPath, requestContent);

            if (!respMsg.IsSuccessStatusCode)
            {
                return serviceResponse;
            }

            var response = await respMsg.Content.ReadAsStringAsync();
            _logger.LogInformation($"RESPONSE: ", response);
            var deserializedObject = JsonConvert.DeserializeObject<BillsPayResponse>(response);

            if (deserializedObject.Status)
            {
                serviceResponse.IsSuccessful = true;
                serviceResponse.SetPayload(deserializedObject);
            }

            return serviceResponse;
        }
    }
}
