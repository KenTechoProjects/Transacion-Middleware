using System;

namespace Middleware.Service.DTOs
{
    public class TransferResponse
    {
        public BeneficiaryStatus BeneficiaryStatus { get; set; }
        public TransactionDetails TransactionDetails { get; set; }
        public string Reference { get; set; }
        public DateTime Date { get; set; }
        public string ResponseCode { get; set; }
      
    }

    public class BeneficiaryStatus
    {
        public bool Attempted { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
    }

    public class TransactionDetails
    {
        public string SourceAccountNumber { get; set; }
        public string SourceAccountName { get; set; }
        public string DestinationAccountID { get; set; }
        public string DestinationAccountName { get; set; }
        public string Narration { get; set; }
        public decimal Amount { get; set; }
        public string DestinationBank { get; set; }
    }

}