using System;
namespace Middleware.Service.Onboarding
{
    public class AuthenticationRequest
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
    }
}
