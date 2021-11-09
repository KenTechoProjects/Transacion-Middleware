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
        private const string UPP_CODE_SUFFIX = "UPP";
        private const string FI_CODE_SUFFIX = "FI";

        private static readonly string CUSTOMER_REFERENCE_KEY = "referenceKey";
        private static readonly string VALIDATION_URL_KEY = "validationPath";
        private static readonly string AMOUNT_FIXED_KEY = "fixed";
        // private static readonly string VALIDATION_STATUS = "complete";
        // private static readonly string CUSTOMER_NAME_KEY = "customer_name";
        // private static readonly string TEXT_TYPE_KEY = "text";
        // private static readonly string HIDDEN_TYPE_KEY = "hidden";

        public PaymentManager(IOptions<BillsPaySettings> provider, HttpClient client, ILoggerFactory logger,
            ICodeGenerator codeGenerator)
        {
            _provider = provider.Value;
            _client = client;//. CreateClient("HttpMessageHandler");
            _client.BaseAddress = new Uri(_provider.BaseAddress);
            //_client.DefaultRequestHeaders.Clear();
            _client.Timeout = TimeSpan.FromSeconds(_provider.RequestTimeout);
            _client.DefaultRequestHeaders.Add(KEY, _provider.AppKey);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _logger = logger.CreateLogger(typeof(PaymentManager));
            _codeGenerator = codeGenerator;
        }


        public async Task<ServiceResponse<IEnumerable<BillerInfo>>> GetBillers()
        {
            return await GetBillers("service/categories/billers?category=non-airtime?");
        }

        public async Task<BAPResponse> GetChannelBillers()
        {
            var response = new BAPResponse();
            var rsp = await _client.GetAsync("channel/billers");
            rsp.EnsureSuccessStatusCode();
            var rawResponse = await rsp.Content.ReadAsStringAsync();
            _logger.LogInformation($"CHANNEL BILLERS: {rawResponse}");
            return response;
        }

        public async Task<BAPResponse> GetBapBillers()
        {
            var response = new BAPResponse();
            var rsp = await _client.GetAsync("service/categories/billers?category=non-airtime");
            rsp.EnsureSuccessStatusCode();
            var rawResponse = await rsp.Content.ReadAsStringAsync();
            _logger.LogInformation($"BAP BILLERS: {rawResponse}");
            return response;
        }

        public async Task<BAPResponse> GetBapIProducts(string slug)
        {
            var response = new BAPResponse();
            var rsp = await _client.GetAsync($"service/i/{slug}");
            rsp.EnsureSuccessStatusCode();
            var rawResponse = await rsp.Content.ReadAsStringAsync();
            _logger.LogInformation($"BAP {slug} I PRODUCTS: {rawResponse}");
            return response;
        }

        public async Task<BAPResponse> GetAirtimeBillers()
        {
            var response = new BAPResponse();
            var rsp = await _client.GetAsync("service/categories/billers?category=airtime?");
            rsp.EnsureSuccessStatusCode();
            var rawResponse = await rsp.Content.ReadAsStringAsync();
            _logger.LogInformation($"BAP AIRTIME BILLERS: {rawResponse}");
            return response;
        }

        public async Task<BAPResponse> Wildcard()
        {
            try
            {

         
            var response = new BAPResponse();
            var rsp = await _client.GetAsync(_provider.WildcardEndpoint);
            rsp.EnsureSuccessStatusCode();
            var rawResponse = await rsp.Content.ReadAsStringAsync();
            _logger.LogInformation($"WILDCARD RESPONSE: {rawResponse}");
            return response;   }
            catch(Exception ex)
            {
                _logger.LogCritical(ex, "Server error occurred in Wildcard in the PaymentManager at {0}", DateTime.UtcNow);
                return new BAPResponse { Error=new ErrorResponse {  ResponseCode=ResponseCodes.GENERAL_ERROR, ResponseDescription="An exception has occurred"} };
            }
        }

        public async Task<BAPResponse> GetBapProducts(string slug)
        {
            var response = new BAPResponse();
            var rsp = await _client.GetAsync($"service/{slug}");
            rsp.EnsureSuccessStatusCode();
            var rawResponse = await rsp.Content.ReadAsStringAsync();
            _logger.LogInformation($"BAP {slug} PRODUCTS: {rawResponse}");
            return response;
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
                    ResponseCode = UPP_CODE_SUFFIX + result.StatusCode,
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
                    ResponseCode = UPP_CODE_SUFFIX + result.StatusCode,
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
                if (p.Form != null && p.Form.Data != null)
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


        public async Task<BAPResponse> PayBill(BasePaymentRequest request, string reference, string payerPhoneNumber, IDictionary<string, string> productParameters)
        {
            var response = new BAPResponse();
            BAPTransaction payload;
            if (request.SourceAccountType == Core.DTO.AccountType.BANK)
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
                    request.PaymentParameters = new Dictionary<string, string>
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
                    ResponseCode = UPP_CODE_SUFFIX
                };
                return response;
            }
            var paymentStatus = Types.GetPaymentStatus(result.Data.Status);
            response.Status = paymentStatus;
            if (paymentStatus == BAPStatus.FAILED)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = FI_CODE_SUFFIX + result.Data.PaymentCode
                };
                return response;
            }
            response.IsSuccessful = true;
            return response;
        }

        public async Task<ServiceResponse<PaymentValidationPayload>> Validate(PaymentValidationRequest request, IDictionary<string, string> productParameters)
        {
            var endpoint = string.Empty;
            if (request.ValidationPath != null)
            {
                endpoint = request.ValidationPath;
            }
            else
            {
                endpoint = productParameters[VALIDATION_URL_KEY];
            }
            var response = new ServiceResponse<PaymentValidationPayload>(false);
            var customerDetails = request.CustomerDetails;
            var referenceKey = productParameters[CUSTOMER_REFERENCE_KEY];
            customerDetails[referenceKey] = request.CustomerReference;
            var msg = JsonConvert.SerializeObject(new { data = customerDetails });
            _logger.LogInformation($"BAP VALIDATION REQUEST, {msg}");
            var rsp = await _client.PostAsync(endpoint, new StringContent(msg, Encoding.UTF8, "application/json"));
            rsp.EnsureSuccessStatusCode();
            var rawResponse = await rsp.Content.ReadAsStringAsync();
            _logger.LogInformation($"BAP VALIDATION RESPONSE, {rawResponse}");
            var result = JsonConvert.DeserializeObject<ValidationResponse>(rawResponse);
            if (!result.Status)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.UPPERLINK_VALIDATION_ERROR
                };
                return response;
            }
            // if (!VALIDATION_STATUS.Equals(result.Result.ValidationStatus, StringComparison.OrdinalIgnoreCase))
            // {
            //     response.Error = new ErrorResponse
            //     {
            //         ResponseCode = ResponseCodes.UPPERLINK_VALIDATION_ERROR
            //     };
            //     return response;
            // }
            // var nameNode = result.Result.Data.Parameters.FirstOrDefault(a => a.Name == CUSTOMER_NAME_KEY && a.Type == TEXT_TYPE_KEY);
            // if (nameNode == null)
            // {
            //     response.Error = new ErrorResponse
            //     {
            //         ResponseCode = ResponseCodes.UPPERLINK_VALIDATION_ERROR
            //     };
            //     return response;
            // }
            var payload = new PaymentValidationPayload();
            var items = new List<PaymentValidationResponse>();
            foreach (var item in result.Result.Data.Parameters)
            {
                items.Add(new PaymentValidationResponse
                {
                    Name = item.Name,
                    Label = item.Label,
                    Type = item.Type,
                    Value = item.Value,
                    Readonly = item.Readonly
                });
            }
            payload.Items = items;
            payload.Command = Util.GetValidationCommand(result.Result.Command);
            payload.ValidationPath = result.Result.Data.Endpoint;
            // {
            //     // AccountName = nameNode.Name,
            //     // TraceParameters = result.Result.Data.Parameters.Where(a => a.Type == HIDDEN_TYPE_KEY).ToDictionary(k => k.Name, v => v.Value)
            // };
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
