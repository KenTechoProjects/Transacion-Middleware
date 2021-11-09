using System;
namespace Middleware.Service.DTOs
{
    public class WalletCompletionResponse
    {
        public string WalletNumber { get; set; }
        public string CustomerName { get; set; }
        public string WalletType { get; set; }
        public bool HasCeiling { get; set; }
        public decimal MaximumTransactionAmount { get; set; }
    }
}
