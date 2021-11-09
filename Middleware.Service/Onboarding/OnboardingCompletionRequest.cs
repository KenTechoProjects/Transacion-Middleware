using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.Onboarding
{
    public class OnboardingCompletionRequest 
    {
        public OnboardingCompletionRequest()
        {
            SecurityQuestions = new List<QuestionAnswers>();
        }
        public string AccountNumber { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
        public string TransactionPin { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
        public ICollection<QuestionAnswers> SecurityQuestions { get; set; }
    }
}
