using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.InterWalletServices
{
    public class InterWalletSchemesResponse
    {
        public IEnumerable<WalletSchemes> WalletSchemes { get; set; }
    }
}
