using System;
namespace Middleware.Service.DTOs
{
    public class DeviceActivationCompletionRequest
    {
        public string UserName { get; set; }
        public string DeviceId { get; set; }
        public string DeviceModel { get; set; }
        public string Otp { get; set; }
    }
}
