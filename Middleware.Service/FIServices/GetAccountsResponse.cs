using System;
using System.Collections.Generic;

namespace Middleware.Service.FIServices
{
    public class GetAccountsResponse : BaseResponse
    {
        public IEnumerable<AccountInformation> Accounts { get; set; }
    }

    public class AccountInformation
    {
        public string CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public string CurrencyCode { get; set; }
        public decimal BookBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Product { get; set; }
        public string ProductCode { get; set; }
        public string AccountName { get; set; }
    }
}