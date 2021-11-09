using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Core.Model
{
    public class ReversalLog
    {
        public long ID { get; set; }
        public long TransactionId { get; set; }
        public string SourceAccountID { get; set; }
        public string DestinationAccountID { get; set; }
        public string Narration { get; set; }
        public decimal Amount { get; set; }
        public string ReversalReference { get; set; }
        public ReversalType ReversalType { get; set; }
        public ReversalStatus ReversalStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public string StatusMessage { get; set; }

        public ReversalLog()
        {
            DateCreated = DateTime.Now;
        }
    }
}



