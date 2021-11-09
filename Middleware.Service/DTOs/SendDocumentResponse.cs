 
using Middleware.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class SendDocumentResponse
    {
        public string AccountNumber { get; set; }
        public string PhoneNumber { get; set; }       
        public string Path { get; set; }
        public DocumentDTO Document { get; set; }
        public DocumentType DocumentType { get; set; }
        public DocumentStatus? DocumentStatus { get; set; }
    }
}




