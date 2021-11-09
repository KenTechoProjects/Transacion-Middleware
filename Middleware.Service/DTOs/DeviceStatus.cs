using System;
namespace Middleware.Service.DTOs
{
    public class DeviceStatus
    {
        public string DeviceId { get; set; }
        public string DeviceModel { get; set; }
        public bool Assigned { get; set; } 
    }
}
