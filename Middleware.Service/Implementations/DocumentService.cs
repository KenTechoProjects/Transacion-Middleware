using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Middleware.Core.DAO;
using Middleware.Core.DTO;
using Middleware.Core.Model;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Middleware.Service.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly ICustomerDAO _customerDAO;
        private readonly IWalletOpeningRequestDAO _requestDAO;
        private readonly ICaseDAO _caseDAO;
        private readonly IImageManager _imageManager;
        private readonly IMessageProvider _messageProvider;
        private readonly IDocumentDAO _documentDAO;
        private readonly ILogger _logger;

        private readonly SystemSettings _settings;
        private const string SELFIE_SURFIX = "_PIC";
        private const string ID_SURFIX = "_ID";
        private const string SIGNATURE_SURFIX = "_SIGN";
        private const string UTILITY_BILL_SURFIX = "_UBILL";
        //private const int CODE_LENGTH = 3;
        public DocumentService(ICustomerDAO customerDAO, IWalletOpeningRequestDAO requestDAO,
            ICaseDAO caseDAO, IImageManager imageManager,
            IMessageProvider messageProvider, IDocumentDAO documentDAO, ILoggerFactory logger, IOptions<SystemSettings> settings)
        {
            _customerDAO = customerDAO;
            _requestDAO = requestDAO;
            _caseDAO = caseDAO;
            _imageManager = imageManager;
            _messageProvider = messageProvider;
            _documentDAO = documentDAO;
            _logger = logger.CreateLogger(typeof(DocumentService));
            _settings = settings.Value;
        }

        public async Task<BasicResponse> CreateRequest(long customerId, DocumentVerificationRequest request, string language)
        {
            var item = await _requestDAO.Find(request.AccountId);
            if (item == null)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                    ResponseCodes.REQUEST_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language));
            }
            var customer = await _customerDAO.Find(customerId);
            if (!customer.IsActive)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.UNAUTHORIZED,
                       ResponseCodes.PROFILE_DEACTIVATED, _messageProvider.GetMessage(ResponseCodes.PROFILE_DEACTIVATED, language));
            }
            if (customer.WalletNumber != request.AccountId)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.INVALID_OBJECT_STATE,
                    ResponseCodes.REQUEST_MISMATCH, _messageProvider.GetMessage(ResponseCodes.REQUEST_MISMATCH, language));
            }
            if (item.Status != WalletOpeningStatus.COMPLETED)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.INVALID_OBJECT_STATE,
                    ResponseCodes.REQUEST_NOT_COMPLETED, _messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_COMPLETED, language));
            }

            var userCase = await _caseDAO.Find(customerId, AccountType.WALLET);

            if (userCase == null)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.INVALID_OBJECT_STATE,
                   ResponseCodes.REQUEST_MISMATCH, _messageProvider.GetMessage(ResponseCodes.REQUEST_MISMATCH, language));

            }

            var token = _documentDAO.Begin();
            _caseDAO.Join(token);

            foreach (var doc in request.Documents)
            {
                if (doc.data == null)
                {
                    continue;
                }
                var reference = Guid.NewGuid().ToString();
                var fileName = doc.data.Type == DocumentType.IDENTIFICATION ? reference : request.AccountId;

                fileName = $"{fileName}.{doc.data.ContentType}";
                var location = await _imageManager.SaveImage(request.AccountId, fileName, doc.data.Type, doc.data.RawData);
                await _documentDAO.Add(new Core.Model.Document
                {
                    Location = location,
                    Type = doc.data.Type,
                    State = DocumentState.NEW,
                    Reference = reference,
                    Case_Id = userCase.Id,
                    ExpiryDate = doc.ExpiryDate,
                    IdentificationType = item.IdType,
                    IdNumber = item.IdNumber,
                    IssuanceDate = doc.IssuanceDate,
                    CustomerId = customerId,
                    Status = DocumentStatus.PENDING
                });
            }

            userCase.State = CaseState.READY;
            await _caseDAO.Update(userCase);

            try
            {
                token.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, " An error occurred in the CreateRequest of the DocumentService ");
                token.Rollback();
            }
            var response = new BasicResponse(true);
            return response;
        }

        public Task<string> GetBiometricTag()
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<CaseInfo>> GetCaseDetails(long customerId, AccountType purpose, string language)
        {
            var item = await _caseDAO.Find(customerId, purpose);
            if (item == null)
            {
                return ErrorResponse.Create<ServiceResponse<CaseInfo>>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                  ResponseCodes.REQUEST_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language));
            }
            var result = new CaseInfo
            {
                AccountId = item.AccountId,
                AccountType = item.AccountType,
                State = item.State,
                Documents = item.Documents.Select(d => new DocumentInfo
                {
                    Reference = d.Reference,
                    State = d.State,
                    Type = d.Type,
                    IdentificationType = d.IdentificationType,
                    Note = d.Note

                }).ToArray()
            };
            var response = new ServiceResponse<CaseInfo>(true);
            response.SetPayload(result);
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<DocumentStatusResponse>>> GetCustomerDocumentsStatusOld(string walletNumber, string language)
        {
            var response = new ServiceResponse<IEnumerable<DocumentStatusResponse>>(false);

            var documentResponse = await _documentDAO.FindByWalletNumber(walletNumber);

            if (!documentResponse.Any())
            {
                response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.REQUEST_NOT_FOUND,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language)}"
                };
                _logger.LogInformation("No customer document found");
                return response;
            }

            //   var documentList = new List<DocumentStatusResponse>();
            var documentList = new List<DocumentStatusResponse>();
            foreach (var doc in documentResponse)
            {    //This added after An error occurred for kyc upload Date:18/06/2021
                if (doc.Status == null)
                {
                    doc.Status = DocumentStatus.PENDING;
                }
                documentList.Add(new DocumentStatusResponse
                {
                    PhoneNumber = doc.PhoneNumber,
                    DocType = (int)doc.Type,
                    Status = doc.Status
                });
            }

            response.SetPayload(documentList);
            response.IsSuccessful = true;
            return response;
        }


        public async Task<ServiceResponse<IEnumerable<DocumentStatusResponse>>> GetCustomerDocumentsStatus(string walletNumber, string language)

        {
            _logger.LogInformation("Inside the GetCustomerDocumentsStatus method of DocumentService");
            var response = new ServiceResponse<IEnumerable<DocumentStatusResponse>>(false);

            try
            {




                #region Old code
                //var documentResponse = await _documentDAO.FindByWalletNumber(walletNumber);

                //if (!documentResponse.Any())
                //{
                //    response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                //    response.Error = new ErrorResponse
                //    {
                //        ResponseCode = ResponseCodes.REQUEST_NOT_FOUND,
                //        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language)}"
                //    };
                //    _logger.LogInformation("No customer document found");
                //    return response;
                //}

                ////   var documentList = new List<DocumentStatusResponse>();
                //var documentList = new List<DocumentStatusResponse>();
                //foreach (var doc in documentResponse)
                //{    //This added after An error occurred for kyc upload Date:18/06/2021
                //    if (doc.Status == null)
                //    {
                //        doc.Status = DocumentStatus.PENDING;
                //    }
                //    documentList.Add(new DocumentStatusResponse
                //    {
                //        PhoneNumber = doc.PhoneNumber,
                //        DocType = (int)doc.Type,
                //        Status = doc.Status
                //    });
                //}

                //response.SetPayload(documentList);
                //response.IsSuccessful = true;
                #endregion
                var customer = await _customerDAO.FindByWalletNumber(walletNumber);
                if (customer != null)
                {
                    var cases = await _caseDAO.Find(customer.Id, AccountType.WALLET);

                    {
                        var documentResponse = cases.Documents;


                        if (!documentResponse.Any())
                        {
                            response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                            response.Error = new ErrorResponse
                            {
                                ResponseCode = ResponseCodes.REQUEST_NOT_FOUND,
                                ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language)}"
                            };
                            _logger.LogInformation("No customer document found");
                            return response;
                        }
                        _logger.LogInformation("Document {doc}", JsonConvert.SerializeObject(documentResponse));
                        //   var documentList = new List<DocumentStatusResponse>();
                        var documentList = new List<DocumentStatusResponse>();
                        foreach (var doc in documentResponse)
                        {    //This added after An error occurred for kyc upload Date:18/06/2021
 
                            if (doc.Status == null && doc.State != DocumentState.INVALID && doc.State!= DocumentState.VALID)
                            {
                                doc.Status = DocumentStatus.PENDING;
                            }

                            else if (doc.State == DocumentState.INVALID)
                            {
                                doc.Status = DocumentStatus.REJECTED;
                            }
                             
                            else if (doc.Status == null && doc.State == DocumentState.VALID)
                            {
                                doc.Status = DocumentStatus.APPROVED;
                            }

                            documentList.Add(new DocumentStatusResponse
                            {
                                PhoneNumber = doc.PhoneNumber,
                                DocType = (int)doc.Type,
                                Status = doc.Status
                            });


                        }


                        response.SetPayload(documentList);
                        response.IsSuccessful = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error occurred in the GetCustomerDocumentsStatus method of DocumentService");
            }


            return response;
        }

        public Task<ServiceResponse<DocumentStatusResponse>> GetDocumentStatus(string phoneNumber, DocumentType docType, string language)
        {
            throw new NotImplementedException();
        }

        public async Task<BasicResponse> UpdateDocument(long customerId, DocumentData documentData, string reference, string language)
        {

            try
            {
                var document = await _documentDAO.Find(customerId, reference);

                if (document == null)
                {
                    return ErrorResponse.Create<BasicResponse>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                        ResponseCodes.REQUEST_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language));
                }
                if (documentData.data == null)
                {
                    return ErrorResponse.Create<BasicResponse>(FaultMode.CLIENT_INVALID_ARGUMENT,
                        ResponseCodes.INPUT_VALIDATION_FAILURE, _messageProvider.GetMessage(ResponseCodes.INPUT_VALIDATION_FAILURE, language));
                }
                if (document.State != DocumentState.INVALID)
                {
                    return ErrorResponse.Create<BasicResponse>(FaultMode.INVALID_OBJECT_STATE,
                        ResponseCodes.DOCUMENT_NOT_UPDATABLE, _messageProvider.GetMessage(ResponseCodes.DOCUMENT_NOT_UPDATABLE, language));
                }

                var documentName = document.Type == DocumentType.IDENTIFICATION ? document.Reference : document.Case.AccountId;

                documentName = $"{documentName}.{documentData.data.ContentType}";
                document.Location = await _imageManager.UpdateImage(document.Case.AccountId, documentName, documentData.data.Type, documentData.data.RawData);
                document.State = DocumentState.UPDATED;
                if (document.Type == DocumentType.IDENTIFICATION)
                {
                    document.IssuanceDate = documentData.IssuanceDate;
                    document.ExpiryDate = documentData.ExpiryDate;

                }
                if (document.Customer_Id > 0)
                {
                    document.CustomerId = document.Customer_Id;
                }
                await _documentDAO.Update(document);

                return new BasicResponse(true);
            }
            catch (Exception ex)
            {
                var response = new BasicResponse(false);
                _logger.LogCritical(ex, "Server error occurred in the UpdateDocument in the DocumentService at {0}", DateTime.UtcNow);

                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
                {
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                        ResponseDescription =
                           _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                    };
                    return response;
                }

                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };

                return response;

            }
        }

        public async Task<BasicResponse> UploadWalletOpeninggDocs(KYCUploadRequest_ request, DocumentType docType, string language)
        {
            var response = new BasicResponse(false);
            var documentLocation = string.Empty;
            try
            {



                if (!request.IsValid(out var problemSource))
                {
                    response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {problemSource}"
                    };
                    return response;
                }
                var customer = await _customerDAO.FindByWalletNumber(request.WalletNumber);
                if (customer == null)
                {
                    return new BasicResponse
                    {
                        Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.CUSTOMER_NOT_FOUND,
                            ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)}"
                        },
                        FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                        IsSuccessful = false
                    };
                }

                #region As In Ghana Reconsider for Senegal
                //Suggestion: possibly not needed for Senegal since the customer might not have an Account number before KYC as explain by Tosin the Frontend Developer

                //if (customer == null)
                //{
                //    response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                //    response.Error = new ErrorResponse
                //    {
                //        ResponseCode = ResponseCodes.CUSTOMER_NOT_FOUND,
                //        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language)}"
                //    };
                //    _logger.LogInformation($"Customer not found on document upload");
                //    return response;
                //}

                //if (customer.AccountNumber != request.AccountNumber)
                //{
                //    response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                //    response.Error = new ErrorResponse
                //    {
                //        ResponseCode = ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH,
                //        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH, language)}"
                //    };
                //    _logger.LogInformation($"Account - Customer mismatch on document upload");
                //    return response;
                //}
                #endregion
                var documentName = string.Empty;

                switch (docType)
                {
                    case DocumentType.IDENTIFICATION:
                        documentName = $"{request.WalletNumber}{ID_SURFIX}.{request.Document.Extension}";
                        //documentPath = $"{pathToSave}{Path.DirectorySeparatorChar}{documentName}"; //$"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{request.WalletNumber}";
                        break;
                    case DocumentType.PICTURE:
                        documentName = $"{request.WalletNumber}{SELFIE_SURFIX}.{request.Document.Extension}";
                        //  documentPath = $"{pathToSave}{Path.DirectorySeparatorChar}{documentName}"; //$"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{request.WalletNumber}";
                        break;
                    case DocumentType.SIGNATURE:
                        documentName = $"{request.WalletNumber}{SIGNATURE_SURFIX}.{request.Document.Extension}";
                        //  documentPath = $"{pathToSave}{Path.DirectorySeparatorChar}{documentName}"; //$"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{request.WalletNumber}";
                        break;
                    case DocumentType.UTILITY_BILL:
                        documentName = $"{request.WalletNumber}{UTILITY_BILL_SURFIX}.{request.Document.Extension}";
                        // documentPath = $"{pathToSave}{Path.DirectorySeparatorChar}{documentName}";// $"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{request.WalletNumber}";
                        break;
                    default:
                        response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                        response.Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.INVALID_DOCUMENT_TYPE,
                            ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_DOCUMENT_TYPE, language)}"
                        };
                        return response;
                };


                documentLocation = await _imageManager.SaveImage(request.WalletNumber, documentName, docType, request.Document.RawData);
                //Save document on server and return the file path (location)
                // var documentFile = await _imageManager.SaveImage(documentPath, documentName, request.Document.RawData);
                _logger.LogInformation("Image Path {0} fordirectory", documentLocation);
                //var documentFile = await _imageManager.SaveImageToWebSite("D", documentName, DocumentType.PICTURE, request.Document.RawData);

                if (string.IsNullOrWhiteSpace(documentLocation))
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.IMAGE_PROFILE_NOT_CREATED,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.IMAGE_PROFILE_NOT_CREATED, language)
                    };
                    return response;
                }
                var case_ = await _caseDAO.Find(customer.Id, AccountType.WALLET);
                //var case_ = await _documentDAO.FindByUnion(customer.Id,AccountType.WALLET);

                long caseId = 0;//(case_ != null && case_.Id > 0) ? case_.FirstOrDefault().Case_Id : 0;

                if (case_ == null)
                {
                    _logger.LogInformation("Customer has no case file created ");


                    var oldRequest = await _requestDAO.Find(customer.WalletNumber);

                    var newCase = new Case
                    {
                        RequestReference = Guid.NewGuid().ToString(),
                        Customer_Id = customer.Id,
                        State = CaseState.NEW,
                        AccountId = customer.WalletNumber,
                        AccountType = AccountType.WALLET,
                        DateOfBirth = oldRequest != null ?
                        oldRequest.BirthDate : DateTime.Now.Date,
                        DateCreated = DateTime.Now,
                        Comments = "Case crated from custmer record since the case file was not created at onboarding"
                    };
                    _logger.LogInformation("Started calling the the CaseDAO.add  method of the CaseDAO  from the Document service Paylad:===>{0} ", JsonConvert.SerializeObject(newCase));
                    caseId = await _caseDAO.Add(newCase);
                    _logger.LogInformation("Finished calling the the CaseDAO.add  method of the CaseDAO  from the Document service Paylad:===>{0} ", caseId);
                    //response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                    //response.Error = new ErrorResponse
                    //{
                    //    ResponseCode = ResponseCodes.INVALID_DOCUMENT_TYPE,
                    //    ResponseDescription = $" No case file was found to attach the document to"
                    //};
                    //return response;
                }
                else
                {
                    caseId = case_.Id;
                }
                var customerDocument = new KYCDocumentDTO();
                if (customer != null)
                {
                    customerDocument = new KYCDocumentDTO
                    {
                        Location = documentLocation,
                        State = DocumentState.NEW,
                        PhoneNumber = request.WalletNumber,
                        CustomerId = customer.Id,
                        Type = docType,
                        Status = DocumentStatus.PENDING,
                        StatusDate = DateTime.Now,
                        DocumentName = documentName,
                        LastUpdateDate = DateTime.Now.Date,
                        Case_Id = caseId,
                        Reference = Guid.NewGuid().ToString()
                    };
                }
                else
                {
                    customerDocument = new KYCDocumentDTO
                    {
                        Location = documentLocation,
                        State = DocumentState.NEW,
                        PhoneNumber = request.WalletNumber,
                        //CustomerId=customer.Id,
                        Type = docType,
                        Status = DocumentStatus.PENDING,
                        StatusDate = DateTime.Now,
                        DocumentName = documentName,
                        LastUpdateDate = DateTime.Now.Date,
                        Case_Id = caseId
                    };

                }

                var isDocExist = await _documentDAO.FindByDocumentType(request.WalletNumber, (DocumentType)(int)docType);
                if (isDocExist == null)
                {
                    _logger.LogInformation("SAVING_NEW_DOCUMENT11111111111111111");
                    await _documentDAO.AddKYC(customerDocument);
                }
                else
                {
                    _logger.LogInformation("UPDATING_EXISTING_DOCUMENT222222222222222222");
                    await _documentDAO.UpdateKYC(customerDocument, true);
                }

                response.IsSuccessful = true;
                response.FaultType = FaultMode.NONE;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "server error occurred in the UploadWalletOpeninggDocs of DocumentService at {0}", DateTime.UtcNow);

                await _imageManager.DeletFileFromFileDirectory(documentLocation);

                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
                {
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                        ResponseDescription =
                            _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                    };
                    return response;
                }

                return new BasicResponse
                {
                    Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.GENERAL_ERROR,
                        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)}"
                    },
                    FaultType = FaultMode.SERVER,
                    IsSuccessful = false
                };
            }
        }


        public async Task<List<DocumentType>> DocumentTypes(string language)
        {
            var data = new List<DocumentType>() { DocumentType.PICTURE, DocumentType.IDENTIFICATION, DocumentType.SIGNATURE, DocumentType.UTILITY_BILL };
            return await Task.FromResult(data);
        }
        private Dictionary<DocumentType, string> GetDocumentnameandPath(List<DocumentTypeAndExtension> docTypes, string walletNumber)
        {
            try
            {
                _logger.LogInformation("Entered inside the GetDocumentnameandPath method of Document Service");
                Dictionary<DocumentType, string> documetNameAndPath = new Dictionary<DocumentType, string>();
                string documentName = string.Empty;
                string documentPath = string.Empty;
                DocumentType documentType;
                _logger.LogInformation("Iterating the DocumentTypeAndExtension  in the GetDocumentnameandPath method of Document Service");


                foreach (var docType in docTypes)
                {
                    switch (docType.DocumentType)
                    {
                        case DocumentType.IDENTIFICATION:
                            documentName = $"{walletNumber}{ID_SURFIX}.{docType.Document.Extension}_{docType.Document.RawData}";
                            documentPath = $"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{walletNumber}";
                            documentType = DocumentType.IDENTIFICATION;
                            break;
                        case DocumentType.PICTURE:
                            documentName = $"{walletNumber}{SELFIE_SURFIX}.{docType.Document.Extension}_{docType.Document.RawData}";
                            documentPath = $"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{walletNumber}";
                            documentType = DocumentType.PICTURE;
                            break;
                        case DocumentType.SIGNATURE:
                            documentName = $"{walletNumber}{SIGNATURE_SURFIX}.{docType.Document.Extension}_{docType.Document.RawData}";
                            documentPath = $"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{walletNumber}";
                            documentType = DocumentType.SIGNATURE;
                            break;
                        case DocumentType.UTILITY_BILL:
                            documentName = $"{walletNumber}{UTILITY_BILL_SURFIX}.{docType.Document.Extension}_{docType.Document.RawData}";
                            documentPath = $"{_settings.DocumentDirectory}{Path.DirectorySeparatorChar}{walletNumber}";
                            documentType = DocumentType.UTILITY_BILL;
                            break;

                    };
                    string dnameType = $"{documentName}-{documentPath}";
                    _logger.LogInformation("Checking  documetNameAndPath dictionary for persistent data");
                    if (!documetNameAndPath.ContainsKey(docType.DocumentType))
                    {
                        documetNameAndPath.Add(docType.DocumentType, dnameType);
                    }

                }
                _logger.LogInformation("Returning data from GetDocumentnameandPath");
                return documetNameAndPath;
            }
            catch (Exception ex)
            {

                _logger.LogCritical(ex, "Compilation of collection of Documentname and extension");
                return null;
            }

        }
        public async Task<BasicResponse> UploadWalletOpeninggDocs(KYCUploadRequest request, string language)
        {
            var response = new BasicResponse(false);
            _logger.LogInformation("Entered inside the UploadWalletOpeninggDocs method of Document Service");
            try
            {


                var docNameType = GetDocumentnameandPath(request.DocumentTypeAndExtensions, request.WalletNumber);
                var customer = await _customerDAO.FindByWalletNumber(request.WalletNumber);
                if (docNameType != null && (docNameType.Count > 0))
                {


                    //  var response = new BasicResponse(false);
                    _logger.LogInformation("Checking for  WalletNumber in the UploadWalletOpeninggDocs method of Document Service");
                    if (!request.IsValid(out var problemSource))
                    {
                        response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                        response.Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                            ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {problemSource}"
                        };
                        return response;
                    }
                    _logger.LogInformation("Checking for  DocumentType Rawdata And Extensions in the UploadWalletOpeninggDocs method of Document Service");

                    if (!request.IsDocumentTypeAndExtensionsValid(out string proSource))
                    {
                        response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                        response.Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                            ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {proSource}"
                        };
                        return response;
                    }

                    var documentName = string.Empty;
                    var documentPath = string.Empty;
                    var rawData = string.Empty;
                    var ext = string.Empty;

                    int countDocTypes = 0;
                    Dictionary<DocumentType, string> docNameExt = new Dictionary<DocumentType, string>();
                    _logger.LogInformation("Iterating for  docNameType in the UploadWalletOpeninggDocs method of Document Service");

                    foreach (var doc in docNameType)
                    {

                        countDocTypes++;
                        string docNamePath = doc.Value;
                        string docPath = string.Empty;
                        if (!doc.Equals(default(KeyValuePair<DocumentType, string>)))
                        {
                            ext = docNamePath.Split("-")[0].Split("_")[1];
                            documentName = docNamePath.Split("-")[0].Split("_")[0] + "." + ext;

                            rawData = docNamePath.Split("-")[0].Split("_")[2];
                            documentPath = docNamePath.Split("-")[1];

                        }
                        DocumentType docType = doc.Key;
                        //Save document on server and return the file path (location)
                        var documentFile = await _imageManager.SaveImage(documentPath, documentName, rawData);


                        var customerDocument = new KYCDocumentDTO();
                        if (customer != null)
                        {
                            _logger.LogInformation("Compiling model for creating Document properties with Customer Id the UploadWalletOpeninggDocs method of Document Service");

                            customerDocument = new KYCDocumentDTO
                            {
                                Location = documentFile,
                                State = DocumentState.NEW,
                                PhoneNumber = request.WalletNumber,
                                //To be reactivated whenever the database access is available
                                // CustomerReference = customer.CustomerReference,
                                CustomerId = customer.Id,
                                Type = docType,
                                Status = DocumentStatus.PENDING,
                                StatusDate = DateTime.Now,
                                DocumentName = documentName
                            };
                        }
                        else
                        {
                            _logger.LogInformation("Compiling model for creating Document properties with no Customer Id the UploadWalletOpeninggDocs method of Document Service");

                            customerDocument = new KYCDocumentDTO
                            {
                                Location = documentFile,
                                State = DocumentState.NEW,
                                PhoneNumber = request.WalletNumber,

                                Type = docType,
                                Status = DocumentStatus.PENDING,
                                StatusDate = DateTime.Now,
                                DocumentName = documentName
                            };

                        }
                        _logger.LogInformation("Checking if  the {0} already exists", docType);
                        var isDocExist = await _documentDAO.FindByDocumentType(request.WalletNumber, (DocumentType)(int)docType);
                        if (isDocExist == null)
                        {
                            _logger.LogInformation("SAVING_NEW_DOCUMENT11111111111111111");
                            bool saved = await _documentDAO.AddKYC(customerDocument);
                            if (saved == true)
                            {
                                response.FaultType = FaultMode.NONE;
                            }
                            else
                            {
                                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                            }
                        }
                        else
                        {
                            _logger.LogInformation("UPDATING_EXISTING_DOCUMENT222222222222222222");
                            bool updated = await _documentDAO.UpdateKYC(customerDocument);
                            if (updated == true)
                            {
                                response.FaultType = FaultMode.NONE;
                            }
                            else
                            {
                                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                            }
                        }
                        docNameExt.Add(doc.Key, doc.Value);


                    }
                    if (docNameExt.Count == countDocTypes)
                    {
                        _logger.LogInformation("Returning result for the UploadWalletOpeninggDocs method of Document Service");

                        response.IsSuccessful = true;
                        return response;
                    }
                    else
                    {
                        response.IsSuccessful = false;

                        return response;
                    }

                }
                else
                {
                    if (docNameType == null)
                    {


                        return new BasicResponse { Error = new ErrorResponse { ResponseCode = ResponseCodes.GENERAL_ERROR, ResponseDescription = "The Document type and extension can not be null or empty" }, FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND, IsSuccessful = false };

                    }
                }
                return new BasicResponse { Error = new ErrorResponse { ResponseCode = ResponseCodes.GENERAL_ERROR, ResponseDescription = "UploadWalletOpeninggDocs was not successful" }, FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND, IsSuccessful = false };
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error ocurred inside UploadWalletOpeninggDocs");
                _logger.LogError(ex.ToString());
                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
                {
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                        ResponseDescription =
                            _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                    };
                    return response;
                }
                return new BasicResponse { Error = new ErrorResponse { ResponseCode = ResponseCodes.GENERAL_ERROR, ResponseDescription = "UploadWalletOpeninggDocs was not successful" }, FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND, IsSuccessful = false };

            }
        }

        public async Task<ServiceResponse<string>> ViewDocument(string walletNumber, DocumentType documentType, string language)
        {
            _logger.LogInformation("Inside the ViewDocument metod of the Document service");
            var response = new ServiceResponse<string>(false);
            try
            {
                if (string.IsNullOrWhiteSpace(walletNumber))
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)
                    };
                    return response;
                }

                _logger.LogInformation("Started calling the  _requestDAO.Find inside the ViewDocument metod of the Document service");
                var result = await _requestDAO.Find(walletNumber);
                _logger.LogInformation("Finished calling the  _requestDAO.FindInside the ViewDocument metod of the Document service. Response {0}", JsonConvert.SerializeObject(result));
                var photoUrl = result?.PhotoLocation;
                if (!string.IsNullOrWhiteSpace(photoUrl))
                {
                    string returnedUrl = string.Empty;
                    if (documentType == DocumentType.PICTURE)
                    {
                        returnedUrl = _settings.PhotoReturnedBaseUrl;
                    }
                    else if (documentType == DocumentType.IDENTIFICATION)
                    {
                        returnedUrl = _settings.DocumentReturnedBaseUrl;
                    }

                    var respon = _imageManager.GetImage(photoUrl, returnedUrl);

                    if (!string.IsNullOrWhiteSpace(respon))
                    {

                        var bse64String = _imageManager.ConvertImageURLToBase64(respon);
                        response.SetPayload(bse64String);
                        response.IsSuccessful = true;
                        _logger.LogInformation("Document was gotten");
                    }
                    else
                    {
                        response.Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.INVALID_DOCUMENT_TYPE,
                            ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_DOCUMENT_TYPE, language)
                        };

                        _logger.LogInformation("Document was not fetched");
                    }

                }
                else
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.REQUEST_NOT_FOUND,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language)
                    };
                    _logger.LogInformation("Document url was not fetched");
                }
            }
            catch (Exception ex)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };
                _logger.LogCritical(ex, "An error occurred");
            }

            return response;
        }

    }
}
