using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class UnapprovedDocumentResponse
    {
        public string CustomerName { get; set; }
        public string AccountNumber { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
