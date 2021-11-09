using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Newtonsoft.Json;

namespace Middleware.Service.FIServices
{
    public class FGTClient : BaseClient, ISubsidiaryService
    {
        private readonly FISettings _fiSettings;
        private readonly HttpClient _client;
        private readonly SystemSettings _settings;
        private readonly ILogger _logger;
        public FGTClient(IOptions<FISettings> fiSettingProvider, IOptions<SystemSettings> settingsProvider,
            IHttpClientFactory factory, ILoggerFactory logger) : base(logger)
        {
            _fiSettings = fiSettingProvider.Value;
            _settings = settingsProvider.Value;
            _logger = logger.CreateLogger(typeof(FGTClient));
            _client = factory.CreateClient("HttpMessageHandler");
            _client.BaseAddress = new Uri(_fiSettings.BaseAddress);
            _client.DefaultRequestHeaders.Add("AppId", _fiSettings.AppId);
            _client.DefaultRequestHeaders.Add("AppKey", _fiSettings.AppKey);

        }
        public async Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber, string subsidiaryID)
        {
            var result= await GetFBNAccountName(accountNumber, subsidiaryID, _client, _fiSettings.InquiryPath,""); ;
            return result;
        }

        public async Task<ServiceResponse<CrossBorderTransferCharge>> GetCharges(decimal amount)
        {
            //Default implementation has zero charge
            var charge = new CrossBorderTransferCharge(0, 0, 0);
            var response = new ServiceResponse<CrossBorderTransferCharge>(true);
            response.SetPayload(charge);
            return await  Task.FromResult(response);
        }

        public async Task<ServiceResponse<ForexRate>> GetForexRate(string sourceCurrency, string targetCurrency)
        {
            var response = new ServiceResponse<ForexRate>(false);
            var request = new RateConversionRequest(_settings.CountryId, sourceCurrency, targetCurrency)
            {
                RequestId = GenerateReference()
            };

            var serviceResponse = await PostMessage<RateConversionResponse, RateConversionRequest>(request, _fiSettings.FGTRatePath, _client);
            if (!serviceResponse.IsSuccessful())
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = $"{_prefix}{serviceResponse.ResponseCode}"
                };
                return response;
            }
            response.IsSuccessful = true;
            response.SetPayload(new ForexRate
            {
                Rate = serviceResponse.CurrentRate,
                Source = sourceCurrency,
                Target = targetCurrency
            });
            return response;
        }

        public async Task<IEnumerable<Subsidiary>> GetSubsidiaries()
        {
            var request = new BaseRequest(_settings.CountryId)
            {
               
                RequestId = GenerateReference()
            };
            _logger.LogInformation("Inside the GetSubsidiaries of FGTClient class");
            _logger.LogInformation("Request to FI ---------------: {0}",JsonConvert.SerializeObject(new {request, _fiSettings.SubsidiariesListPath, _client }));
            var serviceResponse = await PostMessage<GetSubsidiariesResponse, BaseRequest>(request, _fiSettings.SubsidiariesListPath, _client);
            if (!serviceResponse.IsSuccessful())
            {
                _logger.LogWarning("Could not retrieve list of subsidiaries. {0} - {1}", new[] {serviceResponse.ResponseCode, serviceResponse.ResponseMessage});
                throw new Exception("Service Invocation Failure");
            }
            
            var subsidiaries = new List<Subsidiary>();
            foreach (var item in serviceResponse.Subsidiaries)
            {
                subsidiaries.Add(new Subsidiary
                {
                    CountryID = item.CountryId,
                    SubsidiaryName = item.Name,
                    Currency = item.CurrencyCode
                });
            }
            return subsidiaries;
        }

        public async Task<BasicResponse> Transfer(CrossBorderTransferRequest request, string reference)
        {
            _logger.LogInformation("Inside the Transfer method of the FGTClient");
                  var response = new BasicResponse(false);
            var payload = new FGTTransferRequest
            {
                Amount = request.Amount,
                SourceAccountNumber = request.SourceAccountNumber,
                DestinationAccountNumber = request.DestinationAccountID,
                ClientReferenceId = reference,
                RequestId = GenerateReference(),
                Narration = request.Narration,
                SourceCountryId = _settings.CountryId,
                DestinationCountryId = request.DestinationCountry
            };
            _logger.LogInformation("Request to FI ---------------: {0}", JsonConvert.SerializeObject( payload ));

            var serviceResponse = await PostMessage<BaseResponse, FGTTransferRequest>(payload, _fiSettings.FGTTransferPath, _client);
            if (!serviceResponse.IsSuccessful())
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = $"{_prefix}{serviceResponse.ResponseCode}"
                };
                return response;
            }
            response.IsSuccessful = true;
           
            return response;
        }
    }
}
