using Middleware.Core.DTO;
using Middleware.Core.Model;
using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service
{
    public interface IDashboardIntegrationService
    {
        Task<ServiceResponse<CustomerDetails>> GetCustomerInformaton(string walletNo, string langCode);
        Task<BasicResponse> ActivateDevice(string deviceId, string language);
        Task<BasicResponse> DeactivateDevice(string deviceId, string language);
        Task<ServiceResponse<IEnumerable<CustomerDevice>>> GetCustomerDevices(string walletNo, string langCode);
        Task<BasicResponse> LockProfile(string walletNo, string langCode);
        Task<BasicResponse> ReleaseDevice(string deviceId, string language);
        Task<BasicResponse> UnlockProfile(string walletNo, string langCode);
    }
}
