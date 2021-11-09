using System;

namespace Middleware.Service.FIServices
{
    public class InquiryRequest : BaseRequest
    {
        public string AccountNumber { get; set; }
        public string BankCode { get; set; }
        public InquiryRequest(string countryId, string accountNumber) : base(countryId)
        {
            AccountNumber = accountNumber;
        }
    }
}
