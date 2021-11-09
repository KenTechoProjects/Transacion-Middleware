using Middleware.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Middleware.Service.DTOs
{
  public  class DocumentDTO
    {
        public string RawData { get; set; }
        public string Extension { get; set; }
        [JsonIgnore]
        public string CustomerId { get; set; }
        [JsonIgnore]
        public string WalletNumber { get; set; }
  [JsonIgnore]
        public string Location { get; set; }

  [JsonIgnore]
        public string Reference { get; set; }

  [JsonIgnore]
        public string DocumentName { get; set; }
        [JsonIgnore]
        public string IdNumber { get; set; }
        [JsonIgnore]
        public DateTime? IssuanceDate { get; set; }
        [JsonIgnore]
        public DateTime? ExpiryDate { get; set; }
        [JsonIgnore]
        public long CaseId { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if (string.IsNullOrEmpty(RawData))
            {
                problemSource = "Data";
                return false;
            }
            if (string.IsNullOrEmpty(Extension))
            {
                problemSource = "Extension";
                return false;
            }
            return true;
        }
    }



    public class Document
    {
        public string RawData { get; set; }
        public string Extension { get; set; }
        [JsonIgnore]
        public string CustomerId { get; set; }
        
        public string WalletNumber { get; set; }
        [JsonIgnore]
        public string Location { get; set; }

        [JsonIgnore]
        public string Reference { get; set; }

        [JsonIgnore]
        public string DocumentName { get; set; }  
 
        public string IdNumber { get; set; }
        public DateTime? IssuanceDate { get; set; }
        public DateTime? ExpiryDate { get; set; }       
    
   
       
       







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
