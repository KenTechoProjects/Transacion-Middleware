using System;
using System.ComponentModel;

namespace Middleware.Service.DTOs
{
    public class BankAccount
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public AccountBalance Balance { get; set; }
        public bool IsDebitable { get; set; }
        [DefaultValue("-")]
        public string TransactionCharge { get; set; }

    }

    public class AccountBalance
    {
        public decimal BookBalance { get; set; }
        public decimal AvailableBalance { get; set; }
    }
}
