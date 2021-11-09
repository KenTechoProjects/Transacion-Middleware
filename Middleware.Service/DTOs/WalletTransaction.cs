using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Service.DTOs
{
    public abstract class WalletTransaction
    {
        public decimal Amount { get; set; }
       // public string TransactionReference { get; set; }
        public string Narration { get; set; }
        public string Pin { get; set; }

       
    }


}
