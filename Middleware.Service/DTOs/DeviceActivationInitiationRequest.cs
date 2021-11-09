using System;
namespace Middleware.Service.DTOs
{
    public class DeviceActivationInitiationRequest
    {
        public string UserName { get; set; }
        public string DeviceId { get; set; }
        public Answer Answer { get; set; }
    }
}
