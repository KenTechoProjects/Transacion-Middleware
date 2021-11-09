using System;
namespace Middleware.Service.DTOs
{
    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if (string.IsNullOrEmpty(UserName))
            {
                problemSource = "user name";
                return false;
            }
            if (string.IsNullOrEmpty(Password))
            {
                problemSource = "password";
                return false;
            }
            if (string.IsNullOrEmpty(DeviceId))
            {
                problemSource = "device";
                return false;
            }
            return true;
        }
    }
}
