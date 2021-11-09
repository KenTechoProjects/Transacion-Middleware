using System;
namespace Middleware.Service.DTOs
{
    public class SecretQuestion
    {
        public string Question { get; set; }
        public string Answer { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if (string.IsNullOrEmpty(Question))
            {
                problemSource = "Question";
                return false;
            }
            if (string.IsNullOrEmpty(Answer))
            {
                problemSource = "Answer";
                return false;
            }
            return true;
        }
    }
}
