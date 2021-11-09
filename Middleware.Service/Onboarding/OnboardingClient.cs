using System;
using System.Net.Http;
using System.Threading.Tasks;
using Middleware.Service.DTOs;

namespace Middleware.Service.Onboarding
{
    public class OnboardingClient
    {
        private readonly HttpClient _client;

        public OnboardingClient(HttpClient client)
        {
            _client = client;
        }

        //public async Task<ServiceAuthenticationResponse> Authenticate(string userName, string password)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
