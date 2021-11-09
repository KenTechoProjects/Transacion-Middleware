using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Service.DTOs
{
   public class WalletChargingRequest :  WalletTransaction
    {
        public string SourceWalletId { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string DestinationAccountName { get; set; }
        public string DestinationBranchCode { get; set; }


        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if (string.IsNullOrEmpty(DestinationAccountNumber))
            {
                problemSource = "destination";
                return false;
            }

            if (DestinationAccountNumber.Any(c => char.IsDigit(c) == false))
            {
                problemSource = "destination";
                return false;
            }
            return true;
        }
    }
}
