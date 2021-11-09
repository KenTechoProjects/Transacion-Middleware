using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Middleware.Service.Utilities;

namespace Middleware.Service.SmsService
{
    public class SMSService : INotifier
    {
        private readonly IOptions<SMSProviderSetting> _smsConfigProvider;
        static string _channelId;
        static string _channelKey;
        static string _countryId;
        private const string _successStatusCode = "00";
        readonly ILogger _logger;
        private readonly IMessageProvider _messageManeger;
        public SMSService(IOptions<SMSProviderSetting> smsConfigProvider, ILoggerFactory logger, IMessageProvider messageManeger)
        {
            _smsConfigProvider = smsConfigProvider;
            _messageManeger = messageManeger;
            _channelId = _smsConfigProvider.Value.ChannelId;
            _channelKey = _smsConfigProvider.Value.ChannelKey;
            _countryId = _smsConfigProvider.Value.CountryId;
            _logger = logger.CreateLogger(typeof(SMSService));
        }

        public async Task SendSMS(string accountNumber, string phoneNumber, string message)
        {
            try
            {
                _logger.LogInformation("Inside the SendSMS of the SMSService");
                var request = new SMSRequest
                {
                    RequestId = GenerateReference(),
                    AccountNumber = accountNumber,
                    MobileNo = CleanUpNumber(phoneNumber),
                    Message = message,
                    ChannelId = _channelId,
                    ChannelKey = _channelKey,
                    CountryId = _countryId
                };

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => { return true; }
                };

                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri(_smsConfigProvider.Value.SMSBaseEndPoint);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var requestJson = JsonConvert.SerializeObject(request);
                    _logger.LogInformation("SMS-REQUEST: {0}", requestJson);

                    var response = await client.PostAsync(_smsConfigProvider.Value.SendSMSEndPoint, new StringContent(requestJson, Encoding.UTF8, _smsConfigProvider.Value.RequestFormat));
                    var responseMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("SMS-RESPONSE: {0}", responseMsg);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var smsResponse = JsonConvert.DeserializeObject<SMSResponse>(responseMsg);
                        if (smsResponse.ResponseCode != _successStatusCode)
                        {
                            _logger.LogWarning("Failed to send SMS to {0} for {request.AccountNumber} ;  Response: {1} - {2}", request.MobileNo, smsResponse.ResponseCode, smsResponse.ResponseMessage);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send SMS to {0} for {1} : Response : {2}",
                            request.MobileNo, request.AccountNumber, response.StatusCode);
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,"Error Sending SMS for accountNumber to phoneNumber");
            }
        }
      
        private static string CleanUpNumber(string input)
        {
            input = (new string(input.Where(char.IsDigit).ToArray())).TrimStart('0');
            return input;
        }

        internal string GenerateReference()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
