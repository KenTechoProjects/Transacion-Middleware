using Middleware.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Core.DTO
{
  public  class KYCDocumentDTO
    {
     
        public string PhoneNumber { get; set; }
        public string AccountNumber { get; set; }
        public string CustomerReference { get; set; }
        public long CustomerId { get; set; }
        public DocumentState State { get; set; }
        public DocumentType Type { get; set; }
        public DocumentStatus? Status { get; set; }
        public DateTime StatusDate { get; set; }
        public DateTime LastUpdateDate { get; set; }      
        public string DocumentName { get; set; }
        public long Id { get; set; }
        public string Note { get; set; }
        public string Location { get; set; }
        public string IdNumber { get; set; }
        public DateTime? IssuanceDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public IdentificationType IdentificationType { get; set; }     
        public string Reference { get; set; }
        public string ServerReference { get; set; }
        public long Case_Id { get; set; }
        public Case Case { get; set; }
 

    }
    public class KYCDocuments
    {
        public bool  Approved { get; set; }        

    }

}
