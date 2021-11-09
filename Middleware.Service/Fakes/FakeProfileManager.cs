using System;
using System.Threading.Tasks;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;

namespace Middleware.Service.Fakes
{
    public class FakeProfileManager : IProfileManager
    {
        public Task<BasicResponse> ActivateProfile(string emailAddress, string pin, string password, SecretQuestion[] questions)
        {
            return Task.FromResult(new BasicResponse(true));
        }

        public Task<BasicResponse> ReserveProfile(string emailAddress, string phoneNumber, string firstName,
            string lastName, string middleName)
        {
            return Task.FromResult(new BasicResponse(true));
        }
    }
}
