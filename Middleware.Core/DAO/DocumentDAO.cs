using System;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using Dapper;
using Middleware.Core.Model;
using System.Collections;
using System.Collections.Generic;
using Middleware.Core.DTO;
using Microsoft.Extensions.Logging;
using Middleware.Core.Extensions;
namespace Middleware.Core.DAO
{
    public class DocumentDAO : BaseDAO, IDocumentDAO
    {
        private readonly ILogger _logger;
        public DocumentDAO(IDbConnection connection, ILoggerFactory log) : base(connection)
        {
            _logger = log.CreateLogger(typeof(DocumentDAO));
        }
        //Status= DocumentStatus.PENDING,
        // CustomerId=customerId
        public async Task<Document> Add(Document document)
        {
            var query = @"INSERT INTO Documents
                           (location,
                            [state],
                            [type],
                            reference,
                            case_id,
                            IdNumber,
                            IssuanceDate,
                            ExpiryDate,
                            IdentificationType,PhoneNumber,LastUpdateDate,CustomerId,Status) OUTPUT INSERTED.* 
                          VALUES
                            (@Location,
                             @State,
                             @Type,
                             @Reference,
                             @Case_id,
                             @IdNumber,
                             @IssuanceDate,
                            @ExpiryDate,
                            @IdentificationType,@PhoneNumber,@LastUpdateDate,@CustomerId,@Status)";


            var result = await _connection.QueryFirstOrDefaultAsync<Document>(query, document, UnitOfWorkSession?.GetTransaction());
            return result;
        }
        public async Task<bool> AddKYC(KYCDocumentDTO document)
        {
            var saved = 0;

            try
            {
                var query = @"INSERT INTO Documents
                           (Location,
                            [state],
                            [type],
                            reference,
                            case_id,
                            IdNumber,
                            IssuanceDate,
                            ExpiryDate,
                            IdentificationType,PhoneNumber,LastUpdateDate,DocumentName,CustomerId,Status)
                          VALUES
                            (@Location,
                             @State,
                             @Type,
                             @Reference,
                             @Case_id,
                             @IdNumber,
                             @IssuanceDate,
                            @ExpiryDate,
                            @IdentificationType,@PhoneNumber,@LastUpdateDate,@DocumentName,@CustomerId,@Status)";

                saved = await _connection.ExecuteAsync(query, document, UnitOfWorkSession?.GetTransaction());

                switch (saved)
                {
                    case 1:
                        // UnitOfWorkSession?.Commit();
                        return true;
                    case 0:
                        return false;
                    default:
                        //UnitOfWorkSession?.Rollback();
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "server error occurred in the AddKYC of the DocumentDAO at {0}", DateTime.UtcNow);
                return false;
            }

        }
        public async Task<Document> Find(string reference)
        {
            return await _connection.QueryFirstOrDefaultAsync<Document>(@"SELECT * FROM Documents WHERE reference = @reference",
                new { reference });
        }

        public async Task<Document> Find(long customerId, string reference)
        {
            var sql = "Select d.*, c.* from Documents d, Cases c where c.id = d.case_id and d.reference = @reference";
           // var sql = "Select d.*, c.* from Documents d, Cases c where c.id = d.case_id and d.reference = @reference and c.customer_id = @customerId";
            var result= (await _connection.QueryAsync<Document, Case, Document>(sql,
                (d, c) =>
                {
                    d.Case = c;
                    return d;
                }, new { reference, customerId },
                splitOn: "ID"
                )).FirstOrDefault();
            return result;
        }
        public async Task<IEnumerable<Document>> FindByCaseId(long caseId, DocumentStatus documentStatus)
        {
            return await _connection.QueryAsync<Document>(@"SELECT * FROM Documents WHERE Case_Id = @Case_Id and status = @Status",
                new Document { Case_Id = caseId, Status = documentStatus });
        }
        public async Task<IEnumerable<Document>> FindByCaseId(long caseId, DocumentState documentState)
        {
            return await _connection.QueryAsync<Document>(@"SELECT * FROM Documents WHERE Case_Id = @Case_Id and state = @state",
                new Document { Case_Id = caseId, State = documentState });
        }

        public async Task<KYCDocumentDTO> FindByDocumentType(string walletNumber, DocumentType documentType)
        {

            try
            {
                _logger.LogInformation("Inside the FindByDocumentType of the DocumentDAO  class at {0} ", DateTime.UtcNow);
                string sql = "Select * from Documents where PhoneNumber=@walletNumber and [Type]=@documentType;";
                var data = await _connection.QueryFirstOrDefaultAsync<KYCDocumentDTO>(sql, new { walletNumber, documentType });
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A server error occurred in the FindByDocumentType in the DocumentDAO  class at {0} ", DateTime.UtcNow);
                return null;
            }

        }

        public async Task<IEnumerable<Document>> FindByState(DocumentState documentState)
        {
            var sql = "Select d.*, c.* from Documents d, Cases c where c.id = d.case_id and d.state = @state";
            return (await _connection.QueryAsync<Document, Case, Document>(sql,
                (d, c) =>
                {
                    d.Case = c;
                    return d;
                }, new { state = documentState },
                splitOn: "ID"
                )).ToList();
        }

        public async Task<bool> HasDocuments(long customer_id, DocumentType documentType)
        {
            var result = await _connection.QueryFirstOrDefaultAsync<int>(@"select count(*) from Documents d, Cases c where d.case_id = c.Id and d.type = @documentType and c.customer_id = @customer_id",
                new { customer_id, documentType });
            return result != 0;

        }
        public async Task<bool> HasDocumentsNewCustomers(long customer_id, DocumentType documentType)
        {
            var result = await _connection.QueryFirstOrDefaultAsync<int>(@"select count(*) from Documents d, Cases c where d.case_id = c.Id and d.type = @documentType and c.customer_id = @customer_id",
                new { customer_id, documentType });
            return result != 0;

        }
        public async Task Update(Document document)
        {
            var query = @"UPDATE Documents
                          SET  location = @Location,
                               note = @Note,
                               [state] = @State,
                               [serverreference] = @serverreference,
                               [IdNumber] = @IdNumber,
                               [IssuanceDate] = @IssuanceDate,
                               [ExpiryDate] = @ExpiryDate, DocumentName=@DocumentName,
                               [IdentificationType] = @IdentificationType, Status=@Status

                          WHERE id = @Id";
            var data = await _connection.ExecuteAsync(query, document, UnitOfWorkSession?.GetTransaction());

        }

        public async Task<bool> UpdateKYC(Document document, bool hasCustomerAccount = true)
        {

            try
            {
                _logger.LogInformation("inside the UpdateKYC of the DocumentDAO at {0}", DateTime.UtcNow);

                string sql = string.Empty;

                sql = @"UPDATE Documents 
                        SET Status = @Status,
                        LastUpdateDate=@LastUpdateDate,CustomerId=@CustomerId                                           
                             
                        WHERE PhoneNumber = @PhoneNumber AND type = @Type";

                var data = await _connection.ExecuteAsync(sql, document, UnitOfWorkSession?.GetTransaction());
                switch (data)
                {
                    case 1:
                        return true;
                    case 0:
                        return false;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "server error occurred i the UpdateKYC of DocumentDAO at {0}", DateTime.UtcNow);
                return false;
            }
        }
      public async Task<bool> UpdateKYC(KYCDocumentDTO document, bool hasCustomerAccount = true)
        {

            try
            {
                _logger.LogInformation("inside the UpdateKYC of the DocumentDAO at {0}", DateTime.UtcNow);

                string sql = string.Empty;

                sql = @"UPDATE Documents 
                        SET Status = @Status,
                        LastUpdateDate=@LastUpdateDate                                           
                             
                        WHERE PhoneNumber = @PhoneNumber AND type = @Type";

                var data = await _connection.ExecuteAsync(sql, document, UnitOfWorkSession?.GetTransaction());
                switch (data)
                {
                    case 1:
                        return true;
                    case 0:
                        return false;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "server error occurred i the UpdateKYC of DocumentDAO at {0}", DateTime.UtcNow);
                return false;
            }
        }

        public async Task<IEnumerable<Document>> FindByWalletNumber(string WalletNumber)
        {

            var data = await _connection.QueryAsync<Document>("SELECT * FROM Documents WHERE PhoneNumber = @PhoneNumber",
                                                              new Document { PhoneNumber = WalletNumber });
            return data;


        }

        public async Task<IEnumerable<Document>> FindByPhoneNumber(string PhoneNumber)
        {
            return await _connection.QueryAsync<Document>("SELECT * FROM Documents WHERE PhoneNumber = @PhoneNumber",
                                                                      new Document { PhoneNumber = PhoneNumber });
        }



        public async Task<Document> FindByDocumentType(string PhoneNumber, int Type)
        {
            var result = await _connection.QueryFirstOrDefaultAsync<Document>("SELECT * FROM Documents WHERE PhoneNumber = @PhoneNumber AND Type = @Type",
                                                                      new Document { PhoneNumber = PhoneNumber, Type = (Core.DTO.DocumentType)Type });
            return result;
        }

        public async Task<IEnumerable<Customer>> FindCustomers(DocumentStatus Status)
        {
            //old Query by AY
            //    return await _connection.QueryAsync<Customer>("SELECT DISTINCT cust.accountNumber, cust.phoneNumber, cust.firstName, cust.LastName, cust.dateCreated" +
            //                                                   " FROM Customers cust LEFT JOIN Documents doc ON doc.CustomerId = cust.Id WHERE doc.Status = @Status AND cust.onboardingstatus = 2",
            //                                                             new Document { Status = Status });

            string sql = string.Empty;
            if (Status == DocumentStatus.PENDING)
            {
//             
                sql = " SELECT DISTINCT  cust.walletNumber, cust.FirstName, cust.LastName, cust.DateCreated, cs.customer_id  , doc.customerId,doc.status, doc.id as documentId, cs.id as caseId,cust.walletnumber " +
"FROM Customers cust inner join cases cs on cs.customer_id = cust.id inner join documents doc " +
"on doc.case_id = cs.id AND cust.onboardingstatus = 2 and (doc.status is null or doc.status =@Status);";
            }
            else
            {
                sql = "SELECT DISTINCT  cust.walletNumber, cust.FirstName, cust.LastName, cust.DateCreated, cs.customer_id  , doc.customerId,doc.status, doc.id as documentId, cs.id as caseId ,cust.walletnumber " +
"FROM Customers cust inner join cases cs on cs.customer_id = cust.id inner join documents doc " +
"on doc.case_id = cs.id AND cust.onboardingstatus = 2 and   doc.status =@Status;";


            }



            var data = await _connection.QueryAsync<Customer>(sql, new { Status = Status });
            var dd= data.DistinctBy(p=>p.WalletNumber);
            return dd;
        }


        public async Task<int> CountCustomerDocument(string PhoneNumber, DocumentStatus Status)
        {
            var query = "SELECT COUNT(Id) FROM Documents WHERE PhoneNumber = @PhoneNumber AND Status = @Status";

            //if (Status == DocumentStatus.APPROVED)
            //{
            //    query = "SELECT COUNT(Id) FROM Documents WHERE PhoneNumber = @PhoneNumber AND (Status = @Status or (Status is null)";

            //}
            return await _connection.ExecuteScalarAsync<int>(query, new { PhoneNumber, Status });
        }

        public async Task<IEnumerable<KYCDocuments>> FindByPhoneNumberForDocumentStatuses(string PhoneNumber)
        {
            var data = await _connection.QueryAsync<KYCDocuments>("SELECT * FROM Documents WHERE PhoneNumber = @PhoneNumber",
                                                                  new { PhoneNumber = PhoneNumber });
            return data;
        }

        public async Task<IEnumerable<Document>> FindByUnion(long customerId, long caseId)
        {
 
            
                string sql = "select * from Documents d, Cases c where d.case_id = c.Id and c.Id =@caseId and c.customer_id =@customerId;";
                var result = await _connection.QueryAsync<Document>(sql,
                    new { customerId, caseId }); return result;
            
 

            //string sql = "select * from Documents d, Cases c where d.case_id = c.Id and c.Id =@caseId and c.customer_id =@customerId;";
            //var result = await _connection.QueryAsync<Document>(sql,
            //    new { customerId, caseId }); return result;

 


        }
    }
}
