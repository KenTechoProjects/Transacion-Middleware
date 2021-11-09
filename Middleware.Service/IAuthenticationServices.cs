using System;
using System.Threading.Tasks;
using Middleware.Service.DTOs;
using Middleware.Service.Onboarding;
using Middleware.Service.Utilities;

namespace Middleware.Service
{
    public interface IAuthenticationServices
    {
        Task<ServiceResponse<LoginResponse>> Authenticate(LoginRequest request, string language);
        Task<ServiceResponse<AuthenticatedUser>> ValidateSession(string authToken);
        Task EndSession(string authToken);
        Task<ServiceResponse<SecurityQuestion>> GetSecurityQuestion(string username, string language);
    }
}
