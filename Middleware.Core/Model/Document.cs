using System;
using Middleware.Core.DTO;

namespace Middleware.Core.Model
{
    public class Document
    {
        public long Id { get; set; }
        public string Note { get; set; }
        public string Location { get; set; }
        public string IdNumber { get; set; }
        public DateTime? IssuanceDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public IdentificationType IdentificationType { get; set; }
        public DocumentState State { get; set; }
        //This added after An error occurred for kyc upload Date:18/06/2021
        public DocumentStatus?  Status { get; set; }
        public DocumentType Type { get; set; }
        public string Reference { get; set; }
        public string ServerReference { get; set; }
        public long Case_Id { get; set; }
        public Case Case { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime LastUpdateDate { get; set; }     
        public string CustomerReference { get; set; }
        public long CustomerId { get; set; }      
        public long Customer_Id { get; set; }      
        public DateTime StatusDate { get; set; }
        public string DocumentName { get; set; }
       






        //////////////////////////////////////
        /////From Ghana
        //public string Path { get; set; }
        //public string PhoneNumber { get; set; }
        //public string CustomerReference { get; set; }
        //public long CustomerId { get; set; }
        //public DocumentState State { get; set; }
        //public DocumentType Type { get; set; }
        //public DocumentStatus Status { get; set; }
        //public DateTime StatusDate { get; set; }
    }


}
