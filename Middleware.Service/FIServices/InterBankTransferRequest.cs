using System;
namespace Middleware.Service.FIServices
{
    public class InterBankTransferRequest : BaseRequest
    {
        public InterBankTransferRequest(string countryId) : base(countryId)
        {
        }

        public string SourceAccountNumber { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
        public string BeneficiaryAccountName { get; set; }
        public string BeneficiaryBankCode { get; set; }
        public string BeneficiaryBranchCode { get; set; }
        public string Narration { get; set; }
        public string ClientReferenceId { get; set; }
        public decimal Amount { get; set; }
    }
}
