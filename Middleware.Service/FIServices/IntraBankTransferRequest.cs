using System;

namespace Middleware.Service.FIServices
{ 
    public class IntraBankTransferRequest : BaseRequest
    {
        public IntraBankTransferRequest(string countryId) : base(countryId)
        {
            IsCustomerInduced = "Y";
        }

        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string Narration { get; set; }
        public string ClientReferenceId { get; set; }
        public decimal Amount { get; set; }
        public string IsCustomerInduced { get;  } 
    }
}