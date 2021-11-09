using System;
namespace Middleware.Service.FIServices
{
    public class FGTTransferRequest
    {
        public string RequestId { get; set; }
        public string SourceCountryId { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationCountryId { get; set; }
        public string DestinationAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }
        public string ClientReferenceId { get; set; }
        public const string TransactionType = "BankTransfer";  
    }
}
