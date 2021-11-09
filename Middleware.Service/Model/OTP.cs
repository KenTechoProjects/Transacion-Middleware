using System;
namespace Middleware.Service.Model
{
    public class Otp
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string Salt { get; set; }
        public string Code { get; set; }
        public DateTime DateCreated { get; set; }
        public OtpPurpose Purpose { get; set; }
    }

    public enum OtpPurpose
    {
        WALLET_OPENING = 1,
        DEVICE_SWITCH,
        QUESTIONS_RESET,
        PASSWORD_RESET
    }
}
