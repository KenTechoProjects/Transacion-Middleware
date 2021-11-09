using System;
using Middleware.Core.DTO;

namespace Middleware.Service.DTOs
{
    public class DocumentInfo
    {
        public string Reference { get; set; }
        public string Note { get; set; }
        public IdentificationType IdentificationType { get; set; }
        public DocumentState State { get; set; }
        public DocumentType Type { get; set; }

    }
}
