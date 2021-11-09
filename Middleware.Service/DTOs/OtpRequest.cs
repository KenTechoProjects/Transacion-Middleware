using System;
namespace Middleware.Service.DTOs
{
    public class OtpRequest
    {
        public string DeviceId { get; set; }
        public string UserName { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if (string.IsNullOrEmpty(DeviceId))
            {
                problemSource = "Device ID";
                return false;
            }
            if (string.IsNullOrEmpty(UserName))
            {
                problemSource = "UserName";
                return false;
            }
            return true;
        }
    }
}
