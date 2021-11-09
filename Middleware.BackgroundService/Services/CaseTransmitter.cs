using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Middleware.Core.Model;
using DDL.ProcessMaker.Models;
using DDL.ProcessMaker.Services;

using Case = Middleware.Core.Model.Case;
using Middleware.Core.DTO;
using Microsoft.Extensions.Options;
using Middleware.Core.DAO;
using Microsoft.Extensions.Logging;
//using Topshelf;

namespace Middleware.BackgroundService.Services
{
    public class CaseTransmitter : ICaseTransmitter
    {
        private readonly ProcessMakerSettings _settings;
        private readonly ILogger _logger;
        private readonly ICaseDAO _caseDao;
        private readonly IDocumentDAO _documentDAO;
        readonly DDL.ProcessMaker.Services.Case bpmCase;
        public CaseTransmitter(IOptions<ProcessMakerSettings> options, ILoggerFactory logger,
            ICaseDAO caseDao, IDocumentDAO documentDAO)
        {
            _settings = options.Value;
            _logger = logger.CreateLogger(typeof(CaseTransmitter));
            _caseDao = caseDao;
            _documentDAO = documentDAO;
            bpmCase = new DDL.ProcessMaker.Services.Case(_settings.ServerAddress,
                          _settings.ClientId, _settings.ClientSecret, _settings.UserName,
                          _settings.Password)
            {
                PasspordUid = _settings.DocumentIds.Photo,
                MeansOfIdUid = _settings.DocumentIds.Identification
            };
        }

        public async Task TransmitCases()
        {
            //_logger.LogInformation($"Starting Case Transmission....");

            var cases = await _caseDao.FindByState(CaseState.READY);

            foreach (var c in cases)
            {
                try
                {
                    //_logger.LogInformation($"Processing Case for AccountId : {c.AccountId}");
                    c.State = CaseState.IN_PROGRESS;
                    await _caseDao.Update(c);
                    //_logger.LogInformation($"Updated Case for AccountId : {c.AccountId} to { c.State.ToString()}");

                    c.Documents = await _documentDAO.FindByCaseId(c.Id, DocumentState.NEW);
                    //_logger.LogInformation($"ProcessId : {_settings.ProcessId}; TaskId : {_settings.TaskId}");

                    var response = await bpmCase.CreateCase(_settings.ProcessId, _settings.TaskId);

                    //_logger.LogInformation($"Create Case response for AccountId : {c.AccountId} => {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");

                    if (!(response is CreateCaseResponse))
                    {
                        //Log error
                        //_logger.LogInformation($"Failed to create case, Moving to next case....");

                        continue;
                    }
                    var reference = ((CreateCaseResponse)response).app_uid;
                    c.ServerReference = reference;

                    if (string.IsNullOrEmpty(reference))
                    {
                        //_logger.LogInformation($"No server reference. Moving to next case....");

                        continue;
                    }
                    if (c.AccountType == AccountType.WALLET)
                    {
                        //upload wallet docs (photo and id)
                        var photo = c.Documents.SingleOrDefault(d => d.Type == DocumentType.PICTURE);
                        var photoLocation = photo?.Location;

                        var id = c.Documents.SingleOrDefault(d => d.Type == DocumentType.IDENTIFICATION);
                        var idLocation = id?.Location;

                        var uploadResponse = await bpmCase.UploadDocuments(c.ServerReference, _settings.TaskId, idLocation, photoLocation);
                       // _logger.LogInformation($"Upload cocument response for accountId : {c.AccountId} => {Newtonsoft.Json.JsonConvert.SerializeObject(uploadResponse)}");

                        if (uploadResponse.SuccessfulCount == c.Documents.Count())
                        {
                            var data = new
                            {
                                Reference = c.RequestReference,
                                account_number = c.AccountId,
                                email_address = c.Customer.EmailAddress,
                                first_name = c.Customer.FirstName,
                                last_name = c.Customer.LastName,
                                middle_name = c.Customer.MiddleName,
                                mobile_number = c.AccountId,
                                date_of_birth = c.DateOfBirth.ToString("dd-MMM-yyyy"),
                                id_type = id.IdentificationType.ToString(),
                                Id_number = id.IdNumber,
                                date_of_issuance = id.IssuanceDate.GetValueOrDefault().ToString("dd-MMM-yyyy"),
                                expiry_date = id.ExpiryDate.GetValueOrDefault().ToString("dd-MMM-yyyy"),
                                document_type = id.IdentificationType.ToString(),
                                identification = uploadResponse.IdCardResp,
                                proof_of_address = uploadResponse.ProofOfAddressResp,
                                signature = uploadResponse.SignatureResp,
                                selfie = uploadResponse.PassportResp,
                                gender = c.Customer.Gender,
                                title = c.Customer.Title
                            };
                            var paramsResposnse = await bpmCase.SetCaseVariables(reference, data);
                            //_logger.LogInformation($"Set Case Variables response for AccountId : {c.AccountId} => {Newtonsoft.Json.JsonConvert.SerializeObject(paramsResposnse)}");

                            if (paramsResposnse is Response)
                            {
                                //log error

                                await bpmCase.RouteCase(reference);
                                //_logger.LogInformation($"Routed Case for AccountId : {c.AccountId}");

                                photo.ServerReference = uploadResponse.PassportResp.First()?.appDocUid;
                                photo.State = DocumentState.REVIEW_ONGOING;
                                id.ServerReference = uploadResponse.IdCardResp.First()?.appDocUid;
                                id.State = DocumentState.REVIEW_ONGOING;

                                await _documentDAO.Update(photo);
                                await _documentDAO.Update(id);

                                c.State = CaseState.TRANSMITTED;
                                await _caseDao.Update(c);
                            }
                        }
                    }

                }

                catch (Exception ex)
                {
                    _logger.LogCritical(ex,"Error while transmitting =>{0}", c.AccountId);
                    continue;
                }
            }

          //  await TransmitUpdatedDocuments();

           
        }

        public async Task TransmitUpdatedDocuments()
        {
            //_logger.LogInformation($"Starting Document Update....");

            try
            {
                var documents = await _documentDAO.FindByState(DocumentState.UPDATED);

                foreach (var doc in documents)
                {
                    Case c = doc.Case;
                    //_logger.LogInformation($"Transmitting Updated Documents");
                    //_logger.LogInformation($"Document For Case with AccountId : {c.AccountId}");

                    try
                    {
                        UploadResponse uploadResponse = null;

                        if (doc.Type == DocumentType.IDENTIFICATION)
                        {
                            uploadResponse = await bpmCase.UploadDocuments(c.ServerReference, _settings.TaskId, doc.Location);
                        }
                        else if (doc.Type == DocumentType.PICTURE)
                        {
                            uploadResponse = await bpmCase.UploadDocuments(c.ServerReference, _settings.TaskId, null, doc.Location);
                        }
                        else
                        {
                            //_logger.LogInformation($"Invalid document type");
                            continue;
                        }
                        //_logger.LogInformation($"Update document response for AccountId : {c.AccountId} => {Newtonsoft.Json.JsonConvert.SerializeObject(uploadResponse)}");

                        if (uploadResponse.SuccessfulCount == 1)
                        {

                            var data = new object();
                            if (doc.Type == DocumentType.IDENTIFICATION)
                            {
                                data = new
                                {
                                    id_type = doc.IdentificationType.ToString(),
                                    Id_number = doc.IdNumber,
                                    date_of_issuance = doc.IssuanceDate.GetValueOrDefault().ToString("dd-MMM-yyyy"),
                                    expiry_date = doc.ExpiryDate.GetValueOrDefault().ToString("dd-MMM-yyyy"),
                                    document_type = doc.IdentificationType.ToString(),
                                    identification = uploadResponse.IdCardResp,
                                };
                            }
                            else
                            {
                                data = new
                                {
                                    id_type = doc.IdentificationType.ToString(),
                                    selfie = uploadResponse.PassportResp,
                                };
                            }

                            var paramsResposnse = await bpmCase.SetCaseVariables(c.ServerReference, data);
                            //_logger.LogInformation($"Updated Case Variables response for AccountId : {c.AccountId} => {Newtonsoft.Json.JsonConvert.SerializeObject(paramsResposnse)}");

                            if (paramsResposnse is Response)
                            {
                                //log error
                                await bpmCase.RouteCase(c.ServerReference);
                                //_logger.LogInformation($"Routed Case for AccountId : {c.AccountId}");

                                doc.State = DocumentState.REVIEW_ONGOING;
                                await _documentDAO.Update(doc);
                            }
                            //_logger.LogInformation($"Update document response for accountId : {doc.Case.AccountId} => {Newtonsoft.Json.JsonConvert.SerializeObject(uploadResponse)}");

                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical(e,"Error while transmitting==>{0}", c.AccountId);

                        continue;
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,"Error while get documents for update ");
            }
        }

    }
}
