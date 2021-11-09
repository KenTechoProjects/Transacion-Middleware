using System;
using System.Threading.Tasks;
using Middleware.Service.DTOs;

namespace Middleware.Service
{
    public interface IDeviceService
    {
        Task<ServiceResponse<DeviceStatus>> GetDeviceStatus(string deviceId, string language);
        Task<BasicResponse> InitiateDeviceActivation(DeviceActivationInitiationRequest request, string language);
        Task<BasicResponse> CompleteDeviceActivation(DeviceActivationCompletionRequest request, string language);
    }
}
