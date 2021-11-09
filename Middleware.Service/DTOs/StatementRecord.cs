using System;

namespace Middleware.Service.DTOs
{
    public class StatementRecord
    {
        public DateTime Date {get; set;}
        public decimal Amount {get; set;}
        public string Description {get; set;}
        public bool IsCredit { get; set; }
        public DateTime PostedDate { get; set; }
        //Added new
        public DateTime DateOfTransaction { get; set; }
        public string SourceAccount { get; set; }
        public string DestinationAccount { get; set; }
        public string CustomerId { get; set; }
        public string WalletNumber { get; set; }
        public string AccountNumber { get; set; }
    }
}