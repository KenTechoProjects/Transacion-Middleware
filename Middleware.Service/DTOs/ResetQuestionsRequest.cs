using System;

namespace Middleware.Service.DTOs
{
    public class ResetQuestionsRequest
    {
        public string Username { get; set; }
        public string Otp { get; set; }
        public SecretQuestion[] SecretQuestions { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
           
            if (string.IsNullOrEmpty(Otp))
            {
                problemSource = "OTP";
                return false;
            }

            if(SecretQuestions == null || SecretQuestions.Length != 2)
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
