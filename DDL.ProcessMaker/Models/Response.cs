using System;
using System.Collections.Generic;
using System.Text;

namespace DDL.ProcessMaker.Models
{
    public class Response
    {
        public string code { get; set; }
        public string message { get; set; }
    }
    public class ErrorResponse
    {
        public Error error { get; set; }
    }

    public class Error
    {
        public int code { get; set; }
        public string message { get; set; }
    }
    public class CreateCaseResponse
    {
        public string app_uid { get; set; }
        public int app_number { get; set; }
    }

    public class FileUploadResponse
    {
        public string app_doc_uid { get; set; }
        public string app_doc_filename { get; set; }
        public string doc_uid { get; set; }
        public string app_doc_version { get; set; }
        public string app_doc_create_date { get; set; }
        public string app_doc_create_user { get; set; }
        public string app_doc_type { get; set; }
        public string app_doc_index { get; set; }
        public string app_doc_link { get; set; }
    }

    public class UploadResponse
    {
        public int SuccessfulCount { get; set; }
        public int FailedCount { get; set; }
        public string Signature { get; set; }
        public List<DocUploadRequest> SignatureResp { get; set; }
        public string Passport { get; set; }
        public List<DocUploadRequest> PassportResp { get; set; }
        public string IdCard { get; set; }
        public List<DocUploadRequest> IdCardResp { get; set; }
        public string ProofOfAddress { get; set; }
        public List<DocUploadRequest> ProofOfAddressResp { get; set; }
        public string ResidencePermit { get; set; }
        public List<DocUploadRequest> ResidencePermitResp { get; set; }


    }



}
