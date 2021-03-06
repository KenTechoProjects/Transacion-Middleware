using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.InterWalletServices
{

    public class InterWalletTransactionDetails
    {
        public string SourceSchemeCode { get; set; }
        public string SourceWalletId { get; set; }
        public decimal Amount { get; set; }
        public string Narration { get; set; }
        public string DestinationSchemeCode { get; set; }
        public string DestinationWalletId { get; set; }
    }
}
