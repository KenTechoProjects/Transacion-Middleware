using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Middleware.Core.Model
{
    public class Transaction
    {
        public long ID { get; set; }
        public string DestinationCountryId { get; set; }
        public long CustomerId { get; set; }
        public string SourceAccountId { get; set; }
        public string DestinationAccountID { get; set; }
        public string BillerID { get; set; }
        public string Narration { get; set; }
        public string TransactionReference { get; set; }
        public string DestinationInstitution { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime DateCreated { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ResponseTime { get; set; }
        public string ResponseCode { get; set; }
        public string ProviderReference { get; set; }
        //Added to trac for customer's transaction notification
        public TransactionTag SourceTransactionTag { get; set; }
        public TransactionTag DestinationTransactionTag { get; set; }

    }


    public enum TransactionType
    {
        NationalTransfer = 1,
        SelfTransfer,
        SubsidiaryTransfer,
        Airtime,
        BillPayment,
        InterWalletTransfer
    }

    public enum TransactionStatus
    {
        New = 1,
        Pending,
        Successful,
        Failed,
        FIException,
        WalletException,
        InterWalletException
    }
    public enum TransactionTag
    {
        New = 1, Old = 2
    }
}

