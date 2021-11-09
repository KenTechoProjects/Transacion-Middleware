using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Service.DTOs
{
   public class WalletTransferRequest :  WalletTransaction
    {
        public string SourceWalletId { get; set; }
        public string DestinationWalletId { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if (string.IsNullOrEmpty(DestinationWalletId))
            {
                problemSource = "destination";
                return false;
            }

            if (DestinationWalletId.Any(c => char.IsDigit(c) == false))
            {
                problemSource = "destination";
                return false;
            }

            if (string.IsNullOrEmpty(SourceWalletId))
            {
                problemSource = "source";
                return false;
            }

            if (SourceWalletId.Any(c => char.IsDigit(c) == false))
            {
                problemSource = "source";
                return false;
            }

            return true;
        }
    }
}
