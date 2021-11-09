using System;

namespace Middleware.Service.DTOs
{
    public class WalletCompletionRequest
    {
        public string PhoneNumber { get; set; }
        public string Otp { get; set; }
        public string TransactionPin { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
        public string DeviceModel { get; set; }
        public string ReferralCode { get; set; }
        public SecretQuestion[] SecretQuestions { get; set; }
        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;

            if (string.IsNullOrEmpty(PhoneNumber))
            {
                problemSource = "Phone Number";
                return false;
            }

            if (string.IsNullOrEmpty(Otp))
            {
                problemSource = "OTP";
                return false;
            }

            if (string.IsNullOrEmpty(TransactionPin))
            {
                problemSource = "Transaction PIN";
                return false;
            }

            if (string.IsNullOrEmpty(Password))
            {
                problemSource = "Password";
                return false;
            }

            if (string.IsNullOrEmpty(DeviceId))
            {
                problemSource = "Device";
                return false;
            }

            if(SecretQuestions == null || SecretQuestions.Length != 2)
            {
                problemSource = "Secret Questions";
                return false;
            }
            if(SecretQuestions[0].ToString().ToLower()== SecretQuestions.ToString().ToLower())
            {
                problemSource = "Secret Questions";
                return false;
            }
            foreach(var q in SecretQuestions)
            {
                if(!q.IsValid(out problemSource))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
