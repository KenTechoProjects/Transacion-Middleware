using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.DTOs
{
    public class BaseEncryptedRequestDTOStandingInstruction
    {
        public string EncryptedData { get; set; }
        public AuthenticatedUser User { get; set; }
        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;

            if (string.IsNullOrEmpty(EncryptedData))
            {
                problemSource = "Encrypted Data";
                return false;
            }
            if (string.IsNullOrWhiteSpace(User.WalletNumber))
            {
                problemSource = "WalletNumber";
                return false;
            }
            if (string.IsNullOrWhiteSpace(User.BankId))
            {
                problemSource = "BankId";
                return false;
            }
 

            if (User.Id <= 0)
            {
                problemSource = "User Id";
                return false;
            }

            return true;
        }
    }
}
