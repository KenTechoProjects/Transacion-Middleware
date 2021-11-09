using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.Onboarding
{
    public class OnboardingConfigurationProvider
    {
        public string BaseUrl { get; set; }
        public string AppId { get; set; }
        public string AppKey { get; set; }
        public string Question_Reset { get; set; }
        public string Pin_Reset { get; set; }
        public string Auth_Email { get; set; }
        public string Auth_Phone { get; set; }
        public string Reset_Password { get; set; }
        public string Send_OTP { get; set; }
        public string Vlidate_Question { get; set; }
        public string Validate_Pin { get; set; }
   
     public OnboardingEndpoints OnboardingEndpoints { get; set; }
    }

    public class OnboardingEndpoints
    {
        public string Question_Reset { get; set; }
        public string Question_Change { get; set; }

        public string Pin_Reset { get; set; }
        public string Pin_Change { get; set; }

        public string Auth_Email { get; set; }
        public string Auth_Phone { get; set; }

        public string Reset_Password { get; set; }  
        public string Change_Password { get; set; }

        public string Send_OTP { get; set; }
        public string Validate_Answer { get; set; }
        public string Validate_Pin { get; set; }
    
    
      
    }

    //public class OnboardingEndpoints
    //{
    //    public string AuthenticateAccount { get; set; }
    //    public string AuthenticateEmail { get; set; }
    //    public string ValidatePIN { get; set; }
    //    public string ChangePassword { get; set; }
    //    public string ResetPassword { get; set; }
    //    public string ChangePin { get; set; }
    //    public string ResetPin { get; set; }
    //    public string GetSecurityQuestion { get; set; }
    //    public string InitiateOnboarding { get; set; }
    //    public string CompleteOnboarding { get; set; }
    //    public string SendOtp { get; set; }
    //    public string ValidateAnswer { get; set; }
    //    public string Profile { get; set; }

    //}

}
