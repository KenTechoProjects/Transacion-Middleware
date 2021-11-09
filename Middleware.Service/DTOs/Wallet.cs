using System;
namespace Middleware.Service.DTOs
{
    public class Wallet
    {
        public string WalletNumber { get; set; }
        public string WalletType { get; set; }
        public string WalletName { get; set; }
        public AccountBalance Balance { get; set; }
        public string Currency { get; set; }
    }
}
