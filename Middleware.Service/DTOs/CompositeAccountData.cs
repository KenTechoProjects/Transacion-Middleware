using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Middleware.Service.DTOs
{
    public class CompositeAccountData
    {
        public bool HasWallet { get; set; }
        public bool HasAccounts { get; set; }
        public AccountResult AccountData { get; set; }
        public WalletResult WalletData { get; set; }
    
    }

    public class AccountResult
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public IEnumerable<BankAccount> Accounts { get; set; }
    }

    public class WalletResult
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public Wallet Wallet { get; set; }
    }
}
