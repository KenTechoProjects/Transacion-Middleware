using Middleware.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.DTOs
{
    public class TransactionNotificationDTO
    {
       public TransactionsNotificationsDataFinal Credited { get; set; }
       public TransactionsNotificationsDataFinal Debited { get; set; }
    }

    public class TransactionsNotificationsData
    {
     
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public decimal Amount { get; set; }
        [DataType(DataType.Date)]
        public string DateTransacted { get; set; }
        [DataType(DataType.Time)]
        public string TimeTransacted { get; set; }
        public Core.Model.TransactionTag SourceTransactionTag { get; set; }
        public Core.Model.TransactionTag DestinationTransactionTag { get; set; }
     
        public string SourceAcount { get; set; }
        public string DestinationAccount { get; set; }
        //Use Customer Id to check if account was Debited or Dredited
        public bool IsCredited { get;    set; }
        public Core.Model.TransactionType TransactionType { get; set; }
    }
    public class TransactionsNotificationsDataFinal
    {
        public long RecordCount { get; set; }
        public Transactioncategory Transactioncategory { get; set; }
        public List<TransactionsNotificationsData> TransactionsDTOs { get; set; }
    }
    public enum Transactioncategory
    {
        Credit=1,
        Debit=2
    }
}
 