using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Core.DAO;
using Middleware.Core.DTO;
using Middleware.Core.Model;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.Implementations
{
    public class CustomerDocumentService : ICustomerDocumentService
    {
        private readonly IDocumentDAO _documentDAO;
        private readonly ILogger _logger;
        private readonly IMessageProvider _messageProvider;
        private readonly IImageManager _imageManager;
        private readonly SystemSettings _systemSettings;
        private readonly ICustomerDAO _customerDAO;

        private readonly IWalletService _walletService;


        public CustomerDocumentService(IDocumentDAO documentDAO, IMessageProvider messageProvider, IImageManager imageManager, IOptions<SystemSettings> systemSettings,
                                        ICustomerDAO customerDAO, ILoggerFactory logger, IWalletService walletService)
        {
            _documentDAO = documentDAO;
            _messageProvider = messageProvider;
            _imageManager = imageManager;
            _customerDAO = customerDAO;
            _systemSettings = systemSettings.Value;
            _logger = logger.CreateLogger(typeof(CustomerDocumentService));

            _walletService = walletService;
        }

        public async Task<ServiceResponse<SendDocumentResponse>> FetchCustomerDocument(string phoneNumber, DocumentType docType, string language)
        {
            _logger.LogInformation("Inside the FetchCustomerDocument  method of Customer documentservice ");
            var response = new ServiceResponse<SendDocumentResponse>(false);


            try
            {


                var documentResponse = await _documentDAO.FindByDocumentType(phoneNumber, docType);
                if (documentResponse == null)
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
                _logger.LogInformation("Document {doc} ", JsonConvert.SerializeObject(documentResponse));
                // var doc = await _imageManager.GetDocument(documentResponse.Location);

                if (documentResponse.Status == null && documentResponse.State != DocumentState.INVALID && documentResponse.State != DocumentState.VALID)
                {
                    documentResponse.Status = DocumentStatus.PENDING;
                }

                else if (documentResponse.State == DocumentState.INVALID)
                {
                    documentResponse.Status = DocumentStatus.REJECTED;
                }

                else if (documentResponse.Status == null && documentResponse.State == DocumentState.VALID)
                {
                    documentResponse.Status = DocumentStatus.APPROVED;
                }
                var document = new SendDocumentResponse
                {
                    AccountNumber = documentResponse.AccountNumber,
                    PhoneNumber = documentResponse.PhoneNumber,
                    Path = documentResponse.Location,
                    DocumentType = documentResponse.Type,
                    DocumentStatus = documentResponse.Status,
                    Document = new DocumentDTO
                    {
                        RawData = null,     //doc,
                        Extension = _systemSettings.ImageFormat
                    }
                };

                response.SetPayload(document);
                response.IsSuccessful = true;
            }
            catch (Exception ex)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };
                _logger.LogCritical(ex, "An error occured in the FetchCustomerDocument method of Customer document services");
            }
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<SendDocumentResponse>>> FetchCustomerDocuments(string phoneNumber, string language)
        {

            var response = new ServiceResponse<IEnumerable<SendDocumentResponse>>(false);
            try
            {



                var documentResponse = await _documentDAO.FindByWalletNumber(phoneNumber);
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

                var documents = new List<SendDocumentResponse>();
                foreach (var doc in documentResponse)
                {
                    // var doc = await _imageManager.GetDocument(eachDoc.Location);
                    //This added after An error occurred for kyc upload Date:18/06/2021
                    //if (eachDoc.Status == null)
                    //{
                    //    eachDoc.Status = DocumentStatus.PENDING;
                    //}


                    if (doc.Status == null && doc.State != DocumentState.INVALID && doc.State != DocumentState.VALID)
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

                    var document = new SendDocumentResponse
                    {
                        AccountNumber = doc.PhoneNumber,
                        PhoneNumber = doc.PhoneNumber,
                        DocumentType = doc.Type,
                        DocumentStatus = doc.Status,
                        Path = doc.Location,
                        Document = new DocumentDTO
                        {
                            RawData = null,     //doc,
                            Extension = _systemSettings.ImageFormat
                        }
                    };
                    documents.Add(document);
                }

                response.SetPayload(documents);
                response.IsSuccessful = true;

                return response;
            }
            catch (Exception ex)
            {
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
                _logger.LogCritical(ex, "Server error occurred in the FetchCustomerDocuments method of CustomerDocumentService");
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)}"
                };
                return response;
            }
        }

        public async Task<ServiceResponse<IEnumerable<UnapprovedDocumentResponse>>> FetchCustomers(string language)
        {
            var response = new ServiceResponse<IEnumerable<UnapprovedDocumentResponse>>(false);

            var docCustomers = await _documentDAO.FindCustomers(DocumentStatus.PENDING);
            _logger.LogInformation("SEE_CUSTOMER: {0}", JsonConvert.SerializeObject(docCustomers));
            if (!docCustomers.Any())
            {
                response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.REQUEST_NOT_FOUND,
                    ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_FOUND, language)}"
                };
                _logger.LogInformation("No customer found");
                response.SetPayload(null);
                return response;
            }

            var customers = new List<UnapprovedDocumentResponse>();
            foreach (var docCustomer in docCustomers)
            {
                var customerDetail = await _customerDAO.FindByWalletNumber(docCustomer.WalletNumber);
                if (customerDetail == null)
                {
                    response.FaultType = FaultMode.REQUESTED_ENTITY_NOT_FOUND;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.CUSTOMER_NOT_FOUND,
                        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language)}"
                    };
                    _logger.LogInformation("No customer found after finding document for Reference:{0}", docCustomer.WalletNumber);
                    // return response;
                }
                if (customerDetail != null)
                {
                    var customer = new UnapprovedDocumentResponse
                    {
                        CustomerName = $"{customerDetail?.FirstName} {customerDetail?.LastName}",
                        AccountNumber = customerDetail?.WalletNumber,
                        PhoneNumber = docCustomer?.WalletNumber,
                        CreationDate = customerDetail.DateCreated != null ? (DateTime)customerDetail.DateCreated : default(DateTime)
                    };
                    customers.Add(customer);
                }

            }

            response.SetPayload(customers);
            response.IsSuccessful = true;

            return response;
        }

        public async Task<BasicResponse> UpdateCustomerDocument(string phoneNumber, DocumentType docType, bool? status, string language)
        {
            _logger.LogInformation("Inside the UpdateCustomerDocument of CustomerDocumentService ");
            var response = new BasicResponse(false);
            try
            {


                var documentResponse = await _documentDAO.FindByDocumentType(phoneNumber, (int)docType);
                if (documentResponse == null)
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



                //build update doc entity
                //var document = new KYCDocumentDTO
                //{
                //    PhoneNumber = documentResponse.PhoneNumber,
                //    Type = docType,
                //    Status = status == true ? DocumentStatus.APPROVED : DocumentStatus.REJECTED,
                //    StatusDate = DateTime.Now,
                //    LastUpdateDate = DateTime.Now,
                //    State = status == true ? DocumentState.VALID : DocumentState.INVALID
                //};
                if (status != null)
                {
                    documentResponse.Status = status == true ? DocumentStatus.APPROVED : DocumentStatus.REJECTED;
                }
                else
                {
                    documentResponse.Status = DocumentStatus.PENDING;

                }
                var customer = new Customer();
                if (documentResponse.CustomerId == 0)
                {
                      customer = await _customerDAO.FindByWalletNumber(phoneNumber);
                    if (customer != null)
                    {
                        documentResponse.CustomerId = customer.Id;
                        if (string.IsNullOrWhiteSpace(documentResponse.PhoneNumber))
                        {
                            documentResponse.PhoneNumber = customer.WalletNumber;
                        }
                    }


                }


                documentResponse.LastUpdateDate = DateTime.Now;
                documentResponse.State = status == true ? DocumentState.VALID : DocumentState.INVALID;

                //var document = new Document
                //           {
                //               PhoneNumber = documentResponse.PhoneNumber,
                //               Type = docType,
                //               Status = status == true ? DocumentStatus.APPROVED : DocumentStatus.REJECTED,
                //               StatusDate = DateTime.Now
                //           };
                // await _documentDAO.Update(document);
                _logger.LogInformation("Start calling  the _documentDAO.UpdateKYC inside the  UpdateCustomerDocument of CustomerDocumentService. request:====>{0} ", JsonConvert.SerializeObject(documentResponse));
                var rre = await _documentDAO.UpdateKYC(documentResponse);
                _logger.LogInformation(" Finished calling  the _documentDAO.UpdateKYC inside the  UpdateCustomerDocument of CustomerDocumentService. response:====>{0} ", rre);


                //check if all doc types are approved for the customer, then call FI to remove PND
                _logger.LogInformation("  calling  the _documentDAO.CountCustomerDocument inside the  UpdateCustomerDocument of CustomerDocumentService. Request:====>{0} ", JsonConvert.SerializeObject(new { phoneNumber, DocumentStatus.APPROVED }));

                var docCount = await _documentDAO.CountCustomerDocument(phoneNumber, DocumentStatus.APPROVED);
                _logger.LogInformation(" Finished calling  the _documentDAO.CountCustomerDocument inside the  UpdateCustomerDocument of CustomerDocumentService. Request:====>{0} ", docCount);

                if (docCount < 2)
                {
                    response.FaultType = FaultMode.NONE;
                    response.IsSuccessful = true;
                    return response;
                }
                _logger.LogInformation("calling  the _customerDAO.FindByWalletNumber inside the  UpdateCustomerDocument of CustomerDocumentService. request:====>{0} ", JsonConvert.SerializeObject(documentResponse));

                // var customer = await _customerDAO.FindByWalletNumber(phoneNumber);
                _logger.LogInformation(" Finished calling  the _customerDAO.FindByWalletNumber inside the  UpdateCustomerDocument of CustomerDocumentService. Response :====>{0} ", phoneNumber);


                var pndRequest = new UpgradeWalletDTO
                {
                    WalletId = customer.WalletNumber
                };
                //var pndRequest = new PNDRemovalRequest
                //{
                //    AccountNumber = customer.AccountNumber,
                //    FreezeCode = "",
                //    FreezeReason = "",
                //    ClientReferenceId = Guid.NewGuid().ToString(),
                //    RequestId = Guid.NewGuid().ToString(),
                //    CountryId = _systemSettings.CountryId
                //};



                var pndResponse = await _walletService.UpgradeWallet(pndRequest);
                if (pndResponse.IsSuccessful || pndResponse.Error.ResponseCode == "WE02")
                {
                    response.IsSuccessful = true;
                    response.FaultType = FaultMode.NONE;
                }
                else
                {
                    response.Error = pndResponse.Error;
                    response.FaultType = pndResponse.FaultType;
                }
            }
            catch (Exception ex)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };
                _logger.LogCritical(ex, "An error occured in the UpdateCustomerDocument method of Customer document services");
            }
            return response;
        }

    }
}
