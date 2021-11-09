using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Core.Model
{
    public class Reversal
    {
        public long ID { get; set; }
        public long TransactionId { get; set; }
        public string ReversalReference { get; set; }
        public ReversalType ReversalType { get; set; }
        public ReversalStatus ReversalStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastTryDate { get; set; }
        public int RetryCount { get; set; }
        public string StatusMessage { get; set; }
        public virtual Transaction Transaction { get; set; }

        public Reversal()
        {
            DateCreated = DateTime.Now;
        }
    }
    public enum ReversalType
    {
        Wallet = 1,
        Account
    }

    public enum ReversalStatus
    {
        Pending = 1,
        InProcess,
        Failed,
        Complete
    }
}



