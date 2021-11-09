using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Core.DTO;
using Middleware.Core.Model;


namespace Middleware.Core.DAO
{
    public interface IDocumentDAO : ITransactionCoordinator
    {
        
        Task<Document> Add(Document document);
        Task<bool> AddKYC(KYCDocumentDTO document);
        Task Update(Document document);
        Task<bool> UpdateKYC(KYCDocumentDTO document, bool hasCustomerAccount = true);
        Task<bool> UpdateKYC(Document document, bool hasCustomerAccount = true);
        Task<Document> Find(string reference);
        Task<IEnumerable<Document>> FindByCaseId(long caseId, DocumentState documentState);
        Task<IEnumerable<KYCDocuments>> FindByPhoneNumberForDocumentStatuses(string PhoneNumber);
        Task< KYCDocumentDTO> FindByDocumentType(string walletNumber, DocumentType documentType);
        Task<IEnumerable<Document>> FindByState(DocumentState documentState);
        Task<Document> Find(long customerId, string reference);
        Task<bool> HasDocuments(long customer_id, DocumentType documentType);
        Task<IEnumerable<Document>> FindByWalletNumber(string WalletNumber);     
        Task<Document> FindByDocumentType(string phoneNumber, int docType);
        Task<IEnumerable<Document>> FindByPhoneNumber(string phoneNumber);
        Task<IEnumerable<Customer>> FindCustomers(DocumentStatus Status);       
        Task<int> CountCustomerDocument(string PhoneNumber, DocumentStatus Status);
 
        Task<IEnumerable<Document>> FindByCaseId(long caseId, DocumentStatus documentStatus);
 
        Task<IEnumerable<Document>> FindByUnion(long customerId, long caseId);


    }
}
