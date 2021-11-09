using System;
using System.Threading.Tasks;
using Middleware.Service.DTOs;


namespace Middleware.Service.Processors
{
    public interface IProfileManager
    {
        Task<BasicResponse> ReserveProfile(string emailAddress, string phoneNumber,
            string firstName, string lastName, string middleName);
        Task<BasicResponse> ActivateProfile(string phoneNumber, string pin, string password,
            SecretQuestion[] questions);
    }
}
