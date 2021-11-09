using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.WalletServices
{
    public class CreateWalletResponse
    {
        public decimal CurrentLimit { get; set; }
        public bool IsRestricted { get; set; }
        public string WalletId { get; set; }
        public string CustomerName { get; set; }
        public string WalletType { get; set; }

    }
}
