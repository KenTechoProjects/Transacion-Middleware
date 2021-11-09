using DDL.ProcessMaker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DDL.ProcessMaker.Services
{
    public class Case
    {
        private string _url;
        private string _clientId;
        private string _clientSecret;
        private string _username;
        private string _password;

        public string MeansOfIdUid;
        public string PasspordUid;
        public string ProofOfAddressUid;
        public string SignatureUid;
        public string ResidencePermitUid;

        public Case(string baseUrl, string clientId, string clientSecret, string username, string password)
        {
            _url = baseUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _username = username;
            _password = password;
        }
        public async Task<object> CreateCase(string processUid, string taskUid)
        {
            var endpoint = "workflow/cases";
            var method = HttpMethod.Post;
            var restHelper = new RestHelper(_url, _clientId, _clientSecret, _username, _password );

            object body = new { pro_uid = processUid, tas_uid = taskUid };
            var response = await restHelper.Do(endpoint, method, body);

            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var resp = JsonConvert.DeserializeObject<CreateCaseResponse>(result);
                return resp;
            }
            else
            {
                var resp = JsonConvert.DeserializeObject<ErrorResponse>(result);
                return resp;
            }

        }
        
        public async Task<object> SetCaseVariables(string appUid, object body)
        {
            var endpoint = $"workflow/cases/{appUid}/variable";
            var method = HttpMethod.Put;
            var restHelper = new RestHelper(_url, _clientId, _clientSecret, _username, _password);
             
            var response = await restHelper.Do(endpoint, method, body);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new Response { code = "00", message = "Update Successful" };
            }
            else
            {
                return JsonConvert.DeserializeObject<ErrorResponse>(await response.Content.ReadAsStringAsync());
           
            }

        }
        public async Task<UploadResponse> UploadDocuments(string appUid, string taskId, string idMeans =null, string passport = null, string proofOfAddress = null, string signature = null, string residencePermit = null)
        {
            var uploadResponse = new UploadResponse();
            if (!string.IsNullOrWhiteSpace(idMeans))
            {
                var resp = await UploadDocument(appUid, taskId, MeansOfIdUid, (idMeans));
                if (resp is FileUploadResponse)
                {
                    uploadResponse.IdCard = "ID uploaded Successfully";
                    uploadResponse.IdCardResp = new List<DocUploadRequest>();
                    uploadResponse.IdCardResp.Add(new DocUploadRequest
                    {
                        appDocUid = ((FileUploadResponse)resp).app_doc_uid,
                        name = ((FileUploadResponse)resp).app_doc_filename,
                        version = ((FileUploadResponse)resp).app_doc_version
                    });

                    uploadResponse.SuccessfulCount++;
                }
                if (resp is ErrorResponse)
                {
                    uploadResponse.IdCard = "Error Uploading ID - " + ((ErrorResponse)resp).error.message;
                    uploadResponse.IdCardResp = new List<DocUploadRequest>();
                    uploadResponse.FailedCount++;
                }
            }
            if (!string.IsNullOrWhiteSpace(passport))
            {
                var resp = await UploadDocument(appUid, taskId, PasspordUid, passport);
                if (resp is FileUploadResponse)
                {
                    uploadResponse.Passport = " Passport uploaded Successfully";
                    uploadResponse.PassportResp = new List<DocUploadRequest>();
                    uploadResponse.PassportResp.Add(new DocUploadRequest
                    {
                        appDocUid = ((FileUploadResponse)resp).app_doc_uid,
                        name = ((FileUploadResponse)resp).app_doc_filename,
                        version = ((FileUploadResponse)resp).app_doc_version
                    });
                    uploadResponse.SuccessfulCount++;
                }
                if (resp is ErrorResponse)
                {
                    uploadResponse.Passport = "Error Uploading Passport - " + ((ErrorResponse)resp).error.message;
                    uploadResponse.PassportResp = new List<DocUploadRequest>();
                    uploadResponse.FailedCount++;
                }
            }
            if (!string.IsNullOrWhiteSpace(proofOfAddress))
            {
                var resp = await UploadDocument(appUid, taskId, ProofOfAddressUid, proofOfAddress);
                if (resp is FileUploadResponse)
                {
                    uploadResponse.ProofOfAddress = " Proof of Address uploaded Successfully";
                    uploadResponse.ProofOfAddressResp = new List<DocUploadRequest>();
                    uploadResponse.ProofOfAddressResp.Add(new DocUploadRequest
                    {
                        appDocUid = ((FileUploadResponse)resp).app_doc_uid,
                        name = ((FileUploadResponse)resp).app_doc_filename,
                        version = ((FileUploadResponse)resp).app_doc_version
                    });

                    uploadResponse.SuccessfulCount++;
                }
                if (resp is ErrorResponse)
                {
                    uploadResponse.ProofOfAddress = "Error Uploading Proof of Address - " + ((ErrorResponse)resp).error.message;
                    uploadResponse.ProofOfAddressResp = new List<DocUploadRequest>();
                    uploadResponse.FailedCount++;

                }
            }
            if (!string.IsNullOrWhiteSpace(signature))
            {
                var resp = await UploadDocument(appUid, taskId, SignatureUid, signature);
                if (resp is FileUploadResponse)
                {
                    uploadResponse.Signature = " Signature uploaded Successfully";
                    uploadResponse.SignatureResp = new List<DocUploadRequest>();
                    uploadResponse.SignatureResp.Add(new DocUploadRequest
                    {
                        appDocUid = ((FileUploadResponse)resp).app_doc_uid,
                        name = ((FileUploadResponse)resp).app_doc_filename,
                        version = ((FileUploadResponse)resp).app_doc_version
                    }); 
                    uploadResponse.SuccessfulCount++;
                }
                if (resp is ErrorResponse)
                {
                    uploadResponse.Signature = "Error Uploading Signature - " + ((ErrorResponse)resp).error.message;
                    uploadResponse.SignatureResp[0] = new DocUploadRequest();
                    uploadResponse.FailedCount++;
                }
            }
            if (!string.IsNullOrWhiteSpace(residencePermit))
            {
                var resp = await UploadDocument(appUid, taskId, ResidencePermitUid, residencePermit);
                if (resp is FileUploadResponse)
                {
                    uploadResponse.ResidencePermit = " Residence Permit uploaded Successfully";
                    uploadResponse.ResidencePermitResp = new List<DocUploadRequest>();
                    uploadResponse.ResidencePermitResp.Add(new DocUploadRequest
                    {
                        appDocUid = ((FileUploadResponse)resp).app_doc_uid,
                        name = ((FileUploadResponse)resp).app_doc_filename,
                        version = ((FileUploadResponse)resp).app_doc_version
                    });
                    uploadResponse.SuccessfulCount++;
                }
                if (resp is ErrorResponse)
                {
                    uploadResponse.ResidencePermit = "Error Uploading Residence Permit - " + ((ErrorResponse)resp).error.message;
                    uploadResponse.ResidencePermitResp[0] = new DocUploadRequest();
                    uploadResponse.FailedCount++;
                }
            }

            return uploadResponse;
        }

        private async Task<object> UploadDocument(string appUid, string taskId, string docUid, string filePath)
        {
            string endpoint = $"workflow/cases/{appUid}/input-document";
            var restHelper = new RestHelper(_url, _clientId, _clientSecret, _username, _password);
            UploadRequest uploadRequest = new UploadRequest { docUid = docUid, taskId = taskId, comment = " " };
            var method = HttpMethod.Post;
            var response = await restHelper.DoWithFile(endpoint, method, filePath, uploadRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<FileUploadResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                return JsonConvert.DeserializeObject<ErrorResponse>(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<object> RouteCase(string appUid)
        {
            string endpoint = $"workflow/cases/{appUid}/route-case";
            var restHelper = new RestHelper(_url, _clientId, _clientSecret, _username, _password);

            var method = HttpMethod.Put;
            var response = await restHelper.Do(endpoint, method);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new Response { code = "00", message = "Case Routed Successful" };
            }
            else
            {
                return JsonConvert.DeserializeObject<ErrorResponse>(await response.Content.ReadAsStringAsync());
                //an error occurred
            }
        }
    }
}
