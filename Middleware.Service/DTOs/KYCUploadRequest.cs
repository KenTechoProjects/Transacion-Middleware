using Middleware.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Middleware.Service.DTOs
{
    public class KYCUploadRequest
    {

        //public DocumentType DocType { get; set; }
        // public string AccountNumber { get; set; }
        public string WalletNumber { get; set; }
        public List<DocumentTypeAndExtension> DocumentTypeAndExtensions { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;

            if (string.IsNullOrEmpty(WalletNumber))
            {
                problemSource = "WalletNumber";
                return false;
            }
            //if (string.IsNullOrEmpty(AccountNumber))
            //{
            //    problemSource = "AccountNumber";
            //    return false;
            //}
            //if (!Document.IsValid(out problemSource))
            //{
            //    return false;
            //}

            return true;
        }

        public bool IsDocumentTypeAndExtensionsValid(out string problemSource)
        {
            if (DocumentTypeAndExtensions != null && DocumentTypeAndExtensions.Count > 0)
            {
                if (DocumentTypeAndExtensions.Any(p => string.IsNullOrWhiteSpace(p.Document.RawData) || string.IsNullOrWhiteSpace(p.Document.Extension)))
                {
                    problemSource = "one or more raw data and Extension can not be null";
                    return false;
                }
                else
                {
                    problemSource = string.Empty;
                    return true;
                }

            }
            problemSource = "one or more DocumentType and Extension can not be null";
            return false;
        }
    }

    public class KYCUploadRequest_
    {

       // public DocumentType DocType { get; set; }
        // public string AccountNumber { get; set; }
        public string WalletNumber { get; set; }
        public  DocumentDTO Document { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;

            if (string.IsNullOrEmpty(WalletNumber))
            {
                problemSource = "WalletNumber";
                return false;
            }
            //if (string.IsNullOrEmpty(AccountNumber))
            //{
            //    problemSource = "AccountNumber";
            //    return false;
            //}
            if (!Document.IsValid(out problemSource))
            {
                return false;
            }

            return true;
        }

        
    }
    public class DocumentTypeAndExtension
    {
        public DocumentType DocumentType { get; set; }

        public DocumentDTO Document { get; set; }
    }

    public class PhotoUploadResponse
    {

       
        public string WalletNumber { get; set; }
        public string PhotoUrl   { get; set; }
        public bool IsSuccessful { get; set; }

  


    }
}
