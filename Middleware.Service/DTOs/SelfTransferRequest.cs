using System;
namespace Middleware.Service.DTOs
{
    public class SelfTransferRequest
    {
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string DestinationAccountName { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }
        public bool Validate(out string problemSource)
        {
            problemSource = string.Empty;
            if(string.IsNullOrEmpty(SourceAccountNumber))
            {
                problemSource = "Source account number";
                return false;
            }
            if (string.IsNullOrEmpty(DestinationAccountNumber))
            {
                problemSource = "Destination account number";
                return false;
            }
            if(Amount <= 0)
            {
                problemSource = "Amount";
                return false;
            }
            return true;
        }
    }
}
