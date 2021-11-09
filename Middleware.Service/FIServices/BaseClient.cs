using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Middleware.Service.DTOs;
using Middleware.Service.Utilities;
using Newtonsoft.Json;

namespace Middleware.Service.FIServices
{
    public abstract class BaseClient
    {
        internal const string _prefix = "FI";
        readonly ILogger _logger;
        HttpClientResponseTime httpClientResponseTime;
        protected BaseClient(ILoggerFactory logger)
        {
            httpClientResponseTime = new HttpClientResponseTime();
            _logger = logger.CreateLogger(typeof(BaseClient));
        }

        protected static string GenerateReference()
        {
            return Guid.NewGuid().ToString();
        }

        protected async Task<ServiceResponse<dynamic>> GetFBNAccountName(string accountNumber, string countryId, HttpClient client, string requestPath, string banckCode)
        {
            var response = new ServiceResponse<dynamic>(false);
            var request = new InquiryRequest(countryId, accountNumber)
            {
                RequestId = GenerateReference(),
                BankCode = banckCode
            };
            _logger.LogWarning($"FI Request:---------------- { JsonConvert.SerializeObject(request)}");
            var serviceResponse = await PostMessage<InquiryResponse, InquiryRequest>(request, requestPath, client);
            if (!serviceResponse.IsSuccessful())
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning($"FI Name lookup error. Request { JsonConvert.SerializeObject(request)} Response Code = {serviceResponse.ResponseCode}");
                }
                response.Error = new ErrorResponse
                {
                    ResponseCode = $"{_prefix}{serviceResponse.ResponseCode}"
                };
                return response;
            }
            response.IsSuccessful = true;
            _logger.LogInformation(" FI Response  {0} {1}", client.BaseAddress + "/" + requestPath, JsonConvert.SerializeObject(request));
            response.SetPayload(new { serviceResponse.AccountName });
            return response;
        }

        protected async Task<T> PostMessage<T, U>(U request, string path, HttpClient client)
        {
            //  _logger.LogInformation(" PostMessage request from {0} {1}",client.BaseAddress+"/"+path, JsonConvert.SerializeObject(request));
            var input = Util.SerializeAsJson<U>(request);
            _logger.LogInformation($"REQUEST:", input);
            var message = new StringContent(input, Encoding.UTF8, "application/json");
 
            var rawResponse = await client.PostAsync(path, message);
            _logger.LogInformation($"FI Response : {Util.SerializeAsJson(rawResponse)}");

            var body = await rawResponse.Content.ReadAsStringAsync();
            _logger.LogInformation($"FI Response : {body}");
            if (!rawResponse.IsSuccessStatusCode)
            {
                _logger.LogError(" PostMessage reponse from {0} {1}", client.BaseAddress + "/" + path, JsonConvert.SerializeObject(request));
                throw new Exception("Service invocation failure"); //TODO: Replace with gateway error
            }
            _logger.LogInformation("  PostMessage Response {0}", JsonConvert.SerializeObject(body));
            var data = new object();
            if (body != null)
            {
                data = Util.DeserializeFromJson<T>(body);
            
            }  
            return (T)data;
        }

       //protected async Task<Tuple<HttpResponseMessage, TimeSpan>> PostMessageMTest<T, U>(U request, string path, HttpClient client)
       // {
       //     //  _logger.LogInformation(" PostMessage request from {0} {1}",client.BaseAddress+"/"+path, JsonConvert.SerializeObject(request));
       //     var input = Util.SerializeAsJson<U>(request);
            
       //     var rawResponse =await httpClientResponseTime.GetHttpWithTimingInfo( request,true, path);
             
           
       //     return rawResponse;
       // }
        protected async Task<T> PostMessageAwa<T, U>(U request, string path, HttpClient client)
        {
            //  _logger.LogInformation(" PostMessage request from {0} {1}",client.BaseAddress+"/"+path, JsonConvert.SerializeObject(request));
            var input = Util.SerializeAsJson<U>(request);
            _logger.LogInformation($"REQUEST:", input);
            var message = new StringContent(input, Encoding.UTF8, "application/json");
            using (HttpClient ht = new HttpClient())
            {
                var rawResponse = await client.PostAsync("https://app.fbngrop.com/api/path", message);
                _logger.LogInformation($"FI Response : {Util.SerializeAsJson(rawResponse)}");

                var body = await rawResponse.Content.ReadAsStringAsync();
                _logger.LogInformation($"FI Response : {body}");
                if (!rawResponse.IsSuccessStatusCode)
                {
                    _logger.LogError(" PostMessage reponse from {0} {1}", client.BaseAddress + "/" + path, JsonConvert.SerializeObject(request));
                    throw new Exception("Service invocation failure"); //TODO: Replace with gateway error
                }
                _logger.LogInformation("  PostMessage Response {0}", JsonConvert.SerializeObject(body));
                return Util.DeserializeFromJson<T>(body);
            }

        }
    }
}
