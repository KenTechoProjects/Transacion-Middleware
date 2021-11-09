using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.Onboarding
{
    public class InitiatePasswordResetRequest
    {
        public string Username { get; set; }
        public Answer Answer { get; set; }
    }
}
