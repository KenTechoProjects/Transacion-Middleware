using Middleware.Core.DTO;
using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.Processors
{
  public  interface ICustomerDocumentService
    {
        Task<ServiceResponse<SendDocumentResponse>> FetchCustomerDocument(string phoneNumber, DocumentType docType, string language);
        Task<ServiceResponse<IEnumerable<SendDocumentResponse>>> FetchCustomerDocuments(string phoneNumber, string language);
        Task<ServiceResponse<IEnumerable<UnapprovedDocumentResponse>>> FetchCustomers(string language);
        Task<BasicResponse> UpdateCustomerDocument(string phoneNumber, DocumentType docType, bool? status, string language);
    }
}
