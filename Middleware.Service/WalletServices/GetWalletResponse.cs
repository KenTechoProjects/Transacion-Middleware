using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.WalletServices
{
    public class GetWalletResponse
    {
        public string Name { get; set; }
        public string WalletNumber { get; set; }
        public string Currency { get; set; }
        public string AccountName { get; set; }
        public string WalletType { get; set; }
        public decimal Balance { get; set; }

    }
}
