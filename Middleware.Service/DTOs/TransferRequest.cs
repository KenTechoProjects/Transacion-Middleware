using System;
using System.Linq;

namespace Middleware.Service.DTOs
{
    public abstract class TransferRequest
    {
        public string SourceAccountNumber { get; set; }
        public string SourceAccountName { get; set; }
        public string DestinationAccountID { get; set; }
        public string DestinationAccountName { get; set; }
        public string Narration { get; set; }
        public decimal Amount { get; set; }
        public string Pin { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if(string.IsNullOrEmpty(DestinationAccountID))
            {
                problemSource = "destination";
                return false;
            }
            if (DestinationAccountID.Any(c => char.IsDigit(c) == false))
            {
                problemSource = "destination";
                return false;
            }
            return true;
        }

    }
}