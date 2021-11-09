using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.WalletServices
{
    public class WalletStatementResponse
    {
        public List<Statement> statement { get; set; }
    }

    public class Statement
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public string narration { get; set; }
        public string txnstamp { get; set; }
        public string drcrflag { get; set; }
        //Added new
        public DateTime DateOfTransaction { get; set; }
        public string SourceAccount   { get; set; }
        public string DestinationAccount { get; set; }
        public string CustomerId { get; set; }
        public string WalletNumber { get; set; }
        public string AccountNumber { get; set; }
    }
}
