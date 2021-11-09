using Middleware.Core.Model;
using Middleware.Service.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class TransactionsDTO
    {
        public string DestinationCountryId { get; set; }
        public long CustomerId { get; set; }
        public string SourceAccountNumber { get; set; }
        // public string SourceAccountName { get; set; }
        public string DestinationAccountNumber { get; set; }
        // public string DestinationAccountName { get; set; }
        public string BillerID { get; set; }
        // public string BillerName { get; set; }
        public string Narration { get; set; }
        public string TransactionReference { get; set; }
        public string DestinationInstitution { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime DateCreated { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public DateTime ResponseTime { get; set; }
        public string ResponseCode { get; set; }
        public string ProviderReference { get; set; }
        public TransactionTag TransactionTag { get; set; }
    }
}
