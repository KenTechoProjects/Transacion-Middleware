using System;
namespace Middleware.Service.DTOs
{
    public class ServiceAuthenticationResponse
    {
        public string BankId { get; set; }
        public string WalletId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public bool HasBank => !string.IsNullOrEmpty(BankId);
        public bool HasWallet => !string.IsNullOrEmpty(WalletId);
    }
}
