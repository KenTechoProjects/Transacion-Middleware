using System;
using Middleware.Core.DTO;

namespace Middleware.Service.DTOs
{
    public class DocumentData
    {
        public DateTime IssuanceDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public BinaryData data { get; set; }
    }
    public class BinaryData
    {
        public string ContentType { get; set; }
        public int ContentLength { get; set; }
        public string RawData { get; set; }
        public DocumentType Type { get; set; }
    }
}
