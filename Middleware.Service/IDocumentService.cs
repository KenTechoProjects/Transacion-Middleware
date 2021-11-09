using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Core.DTO;
using Middleware.Service.DTOs;

namespace Middleware.Service
{
    public interface IDocumentService
    {
        Task<ServiceResponse<CaseInfo>> GetCaseDetails(long customerId, AccountType purpose, string language);
        Task<BasicResponse> UpdateDocument(long customerId, DocumentData documentData, string reference, string language);
        Task<BasicResponse> CreateRequest(long customerId, DocumentVerificationRequest request, string language);
        Task<ServiceResponse<IEnumerable<DocumentStatusResponse>>> GetCustomerDocumentsStatus(string walletNumber, string language);
        Task<BasicResponse> UploadWalletOpeninggDocs(KYCUploadRequest_ request, DocumentType docType, string language);     
        Task<BasicResponse> UploadWalletOpeninggDocs(KYCUploadRequest request, string language);
        Task<List<DocumentType>> DocumentTypes(string language);     
        Task<ServiceResponse<DocumentStatusResponse>> GetDocumentStatus(string phoneNumber, DocumentType docType, string language);
        Task<string> GetBiometricTag();
        Task<ServiceResponse< string>> ViewDocument(string walletNumber, DocumentType documentType, string language);

    }
}
