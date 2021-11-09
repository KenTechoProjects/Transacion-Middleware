using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.InterWalletServices
{
    public class InterWalletNameEnquiryResponse
    {
        public string ResponseCode { get; set; }
        public string WalletId { get; set; }
        public string WalletName { get; set; }
        public string WalletSchemeCode { get; set; }
    }
}
