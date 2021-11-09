using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.BAP
{
    public class PaymentManager : IPaymentManager
    {
        private readonly BillsPaySettings _provider;
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        static readonly string KEY = "Api-Key";
        private readonly ICodeGenerator _codeGenerator;

        private static readonly string CUSTOMER_REFERENCE_KEY = "referenceKey";
        private static readonly string VALIDATION_URL_KEY = "validationPath";
        private static readonly string AMOUNT_FIXED_KEY = "fixed";
        private static readonly string VALIDATION_STATUS = "complete";
        private static readonly string CUSTOMER_NAME_KEY = "customer_name";
        private static readonly string TEXT_TYPE_KEY = "text";
        private static readonly string HIDDEN_TYPE_KEY = "hidden";

        public PaymentManager(IOptions<BillsPaySettings> provider, IHttpClientFactory factory, ILogger<PaymentManager> logger,
            ICodeGenerator codeGenerator)
        {
            _provider = provider.Value;
            _client = factory.CreateClient("HttpMessageHandler");
            _client.BaseAddress = new Uri(_provider.BaseAddress);
            //_client.DefaultRequestHeaders.Clear();
            _client.Timeout = TimeSpan.FromSeconds(_provider.RequestTimeout);
            _client.DefaultRequestHeaders.Add(KEY, _provider.AppKey);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _logger = logger;
            _codeGenerator = codeGenerator;
        }


        public async Task<ServiceResponse<IEnumerable<BillerInfo>>> GetBillers()
        {
            return await GetBillers("service/categories/billers?category=non-airtime?");
        }

        private async Task<ServiceResponse<IEnumerable<BillerInfo>>> GetBillers(string path)
        {
            var response = new ServiceResponse<IEnumerable<BillerInfo>>(false);
            var rsp = await _client.GetAsync(path);
            rsp.EnsureSuccessStatusCode();
            var rawResponse = await rsp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BillerResponse>(rawResponse);
            if (!result.Status)
            {
                response.Error = new ErrorResponse
                {
                    ResponseDescription = result.Message
                };
                return response;

            }
            var billers = (from c in result.Categories.Where(c => c.Billers.Any())
                           from b in c.Billers.Where(b => b.Enabled == "1")
                           select new BillerInfo
                           {
                               BillerCode = $"{b.Id}`{b.Slug}",
                               BillerName = b.Name
                           }).ToList();
            response.IsSuccessful = true;
            response.SetPayload(billers);
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<ProductInfo>>> GetProducts(string billerCode)
        {
            var response = new ServiceResponse<IEnumerable<ProductInfo>>(false);
            var billerSlug = billerCode.Split('`')[1];
            var rsp = await _client.GetAsync($"service/{billerSlug}");
            rsp.EnsureSuccessStatusCode();
            var rawResponse = await rsp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ProductResponse>(rawResponse);
            if (!result.Status)
            {
                response.Error = new ErrorResponse
                {
                    ResponseDescription = result.Message
                };
                return response;
            }
            var products = new List<ProductInfo>();

            foreach (var p in result.Payload.Products)
            {
                var product = new ProductInfo
                {
                    Price = p.Amount,
                    ProductCode = p.Slug,
                    ProductName = p.Name,
                    IsFixedAmount = AMOUNT_FIXED_KEY.Equals(p.AmountType, StringComparison.OrdinalIgnoreCase)
                };
                if(p.Form != null && p.Form.Data != null)
                {

                }
                
                products.Add(product);
            }
            response.IsSuccessful = true;
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<BillerInfo>>> GetTelcos()
        {
            return await GetBillers("service/categories/billers?category=airtime?");      
        }


        public async Task<BasicResponse> PayBill(BasePaymentRequest request, string reference, string payerPhoneNumber,
            IDictionary<string, string> productParameters)
        {
            var response = new BasicResponse(false);
            BAPTransaction payload;
            if(request.SourceAccountType == Core.DTO.AccountType.BANK)
            {
                payload = new AccountTransaction
                {
                    SourceAccount = request.SourceAccountId
                };
            }
            else
            {
                payload = new WalletTransaction
                {
                    SourceWallet = request.SourceAccountId
                };
            }
            payload.Amount = request.Amount;
            payload.BillerCode = request.BillerCode;
            payload.CustomerPhoneNumber = payerPhoneNumber;
            payload.CustomerReference = request.CustomerReference;
            payload.ProductCode = request.ProductCode;
            payload.RequestReference = reference;

            if (productParameters != null)
            {
                var referenceKey = productParameters[CUSTOMER_REFERENCE_KEY];
                if (request.PaymentParameters != null)
                {
                    request.PaymentParameters[referenceKey] = request.CustomerReference;
                }
                else
                {
                    request.PaymentParameters =  new Dictionary<string, string>
                    {
                        [referenceKey] = request.CustomerReference
                    };
                }
                payload.PaymentParameters = request.PaymentParameters.Select(p => new PayloadItem
                {
                    Name = p.Key,
                    Value = p.Value
                }).ToList();
            }

            var requestData = new { data = payload };
            var msg = JsonConvert.SerializeObject(requestData);

            _logger.LogInformation($"BAP_PAYMENT_REQUEST: {msg}");

            var rsp = await _client.PostAsync("channel/transactions", new StringContent(msg, Encoding.UTF8, "application/json"));
            rsp.EnsureSuccessStatusCode();
            var rawResponse = await rsp.Content.ReadAsStringAsync();
            _logger.LogInformation($"BAP_PAYMENT_RESPONSE: {rawResponse}");
            var result = JsonConvert.DeserializeObject<TransactionResponse>(rawResponse);
            if (!result.Status)
            {
                response.Error = new ErrorResponse
                {
                    //ResponseDescription = result.Message
                    ResponseDescription = rawResponse
                };
                return response;
            }
            response.IsSuccessful = true;
            return response;
        }

        public async Task<ServiceResponse<PaymentValidationResponse>> Validate(PaymentValidationRequest request, IDictionary<string, string> productParameters)
        {
            var endpoint = productParameters[VALIDATION_URL_KEY];
            var response = new ServiceResponse<PaymentValidationResponse>(false);
            var customerDetails = request.CustomerDetails;
            var referenceKey = productParameters[CUSTOMER_REFERENCE_KEY];
            customerDetails[referenceKey] = request.CustomerReference;
            var msg = JsonConvert.SerializeObject(new { data = customerDetails });
            _logger.LogInformation($"BAP validation request, {msg}");
            var rsp = await _client.PostAsync(endpoint, new StringContent(msg, Encoding.UTF8, "application/json"));
            rsp.EnsureSuccessStatusCode();
            var rawResponse = await rsp.Content.ReadAsStringAsync();
            _logger.LogInformation($"BAP validation response, {rawResponse}");
            var result = JsonConvert.DeserializeObject<ValidationResponse>(rawResponse);
            if (!result.Status)
            {
                response.Error = new ErrorResponse
                {
                    ResponseDescription = result.Message
                };
                return response;
            }
            if(!VALIDATION_STATUS.Equals(result.Result.ValidationStatus, StringComparison.OrdinalIgnoreCase))
            {
                response.Error = new ErrorResponse
                {
                    ResponseDescription = result.Message
                };
                return response;
            }
            var nameNode = result.Result.Data.Parameters.FirstOrDefault(a => a.Name ==CUSTOMER_NAME_KEY && a.Type == TEXT_TYPE_KEY);
            if(nameNode == null)
            {
                response.Error = new ErrorResponse
                {
                    ResponseDescription = "Lookup Failed"
                    //TODO: return the right error 
                };
                return response;
            }
            var payload = new PaymentValidationResponse
            {
                AccountName = nameNode.Name,
                TraceParameters = result.Result.Data.Parameters.Where(a => a.Type == HIDDEN_TYPE_KEY).ToDictionary(k => k.Name, v => v.Value)
            };
            response.SetPayload(payload);
            response.IsSuccessful = true;
            return response;
        }

        public string GenerateReference()
        {
            return $"221-DIL-{_codeGenerator.Generate(9)}"; //TODO: Make this configurable
        }
    }
}
