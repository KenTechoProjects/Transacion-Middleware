using System;
using System.Threading.Tasks;
using Middleware.Service.DTOs;
using Middleware.Service.Model;
using Middleware.Service.Onboarding;

namespace Middleware.Service.Processors
{
    public interface IOtpService
    {
        Task<string> CreateOtpMessage(string username, string language, OtpPurpose otpPurpose);
    }
}
