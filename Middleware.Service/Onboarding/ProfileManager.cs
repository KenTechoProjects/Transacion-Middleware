using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;

namespace Middleware.Service.Onboarding
{
    public class ProfileManager : IProfileManager
    {
        private readonly HttpClient _client;
        private const string _prefix = "ONB";
        private readonly ILogger _logger;
        public ProfileManager(IHttpClientFactory factory,
            IOptions<OnboardingConfigurationProvider> onboadingConfigProvider, ILoggerFactory logger)
        {
            _client = factory.CreateClient("HttpMessageHandler");
            _client.BaseAddress = new Uri(onboadingConfigProvider.Value.BaseUrl);
            _client.DefaultRequestHeaders.Add("AppId", onboadingConfigProvider.Value.AppId);
            _client.DefaultRequestHeaders.Add("AppKey", onboadingConfigProvider.Value.AppKey);
            _logger = logger.CreateLogger(typeof(ProfileManager));
        }

        public async Task<BasicResponse> ActivateProfile(string phoneNumber, string pin, string password, SecretQuestion[] questions)
        {

            try
            {
                var data = new
                {
                    phoneNumber,
                    pin,
                    password,
                    securityquestions = questions
                };
                var message = new StringContent(Util.SerializeAsJson(data),
                Encoding.UTF8, "application/json");
                //var response = await _client.PutAsync("customer/profile", message);
                var response = await _client.PostAsync("customer/profile/activate", message);
                if (!response.IsSuccessStatusCode)
                {
                    var rawError = await response.Content.ReadAsStringAsync();
                    var error = Util.DeserializeFromJson<OnboardingError>(rawError);
                    bool isSuccessful = error.Code == "010";

                    return new BasicResponse(false)
                    {
                        Error = new ErrorResponse
                        {
                            ResponseCode = $"{_prefix}{error.Code}", ResponseDescription = error.Message

                        }, IsSuccessful=isSuccessful
                    };
                }

                return new BasicResponse(true);
            }
            catch (Exception ex)
            {


                _logger.LogCritical(ex, "Error occurred in the ActivateProfile of the   ProfileManeger at {0}", DateTime.UtcNow);
                return new BasicResponse(false);
            }

        }

        public async Task<BasicResponse> ReserveProfile(string emailAddress, string phoneNumber,
            string firstName, string lastName, string middleName)
        {
            try
            {
                var data = new
                {
                    firstName=firstName,
                    middleName=middleName,
                    lastName=lastName,
                    email = emailAddress,
                    phoneNumber=phoneNumber
                };
                var message = new StringContent(Util.SerializeAsJson(data),
                    Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("customer/profile", message);
                if (!response.IsSuccessStatusCode)
                {
                    var rawError = await response.Content.ReadAsStringAsync();

                    var error = new OnboardingError();
                    if (rawError != null)
                    {
                        error = Util.DeserializeFromJson<OnboardingError>(rawError);
                    }



                    return new BasicResponse(false)
                    {
                        Error = new ErrorResponse
                        {
                            ResponseCode = $"{_prefix}{error.Code}"

                        }
                    };
                }
                return new BasicResponse(true);
            }
            catch (Exception ex)
            {


                _logger.LogCritical(ex, "Error occurred in the  ReserveProfile of the  ProfileManager at {0}", DateTime.UtcNow);
                return new BasicResponse(false);
            }

        }
    }
}
