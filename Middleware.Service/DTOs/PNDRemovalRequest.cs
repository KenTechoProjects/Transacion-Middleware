using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class PNDRemovalRequest
    {
        public string AccountNumber { get; set; }
        public string FreezeCode { get; set; }
        public string FreezeReason { get; set; }
        public string ClientReferenceId { get; set; }
        public string RequestId { get; set; }
        public string CountryId { get; set; }
    }


}



