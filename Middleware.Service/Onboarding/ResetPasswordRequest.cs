using Middleware.Service.DTOs;
using System;

namespace Middleware.Service.Onboarding
{
    public class ResetPasswordRequest
    {
        public Answer Answer { get; set; }
        public string UserName { get; set; }
        //public string DeviceId { get; set; }
        public string Otp { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }



}
