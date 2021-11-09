using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.InterWalletServices
{
    public class InterWalletTransStatusResponse
    {
        public string TransactionReference { get; set; }
        public string ResponseMessage { get; set; }
        public string ResponseCode { get; set; }
    }
}

