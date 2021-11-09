using DDL.ProcessMaker.Models;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace DDL.ProcessMaker.Services
{
    public class RestHelper
    {
        static HttpClient client;
        private string _baseUrl;
        private string _clientId;
        private string _clientSecret;
        private string _username;
        private string _password;
        public RestHelper(string baseUrl, string clientId, string clientSecret, string username, string password)
        {
            _baseUrl = baseUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _username = username;
            _password = password;

            HttpClientHandler clientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };
            client = new HttpClient(clientHandler);


        }

        public async Task<HttpResponseMessage> Do(string endpoint, HttpMethod method, object body = null, string contentType = "application/json")
        {
            //Authenticate to get token
            var auth = new Authenticate(_baseUrl, _clientId, _clientSecret);
            var token = await auth.GetTokenCode(_username, _password);


            string url = _baseUrl.TrimEnd('/') + "/api/1.0/" + endpoint;
            HttpResponseMessage response = new HttpResponseMessage();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (method == HttpMethod.Get)
            {
                response = await client.GetAsync(url);
            }
            else if (method == HttpMethod.Post)
            {
                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, contentType);
                response = await client.PostAsync(url, content);
            }
            else if (method == HttpMethod.Put)
            {
                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, contentType);
                response = await  client.PutAsync(url, content);
            }

            return response;
        }
        public async Task<HttpResponseMessage> DoWithFile(string endpoint, HttpMethod method, string file, object body = null)
        {
            //Authenticate to get token
            var auth = new Authenticate(_baseUrl, _clientId, _clientSecret);
            var token = await auth.GetTokenCode(_username, _password);


            string url = _baseUrl.TrimEnd('/') + "/api/1.0/" + endpoint;
            HttpResponseMessage response = new HttpResponseMessage();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestContent = new MultipartFormDataContent();


            var fileContent = new ByteArrayContent(await Utility.FileToByteArrayAsync(file));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MimeTypes.GetMimeType(file));
            var request = (UploadRequest)body;

            requestContent.Add(fileContent, "form", file);
            requestContent.Add(new StringContent(request.taskId), "tas_uid");
            requestContent.Add(new StringContent(request.docUid), "inp_doc_uid");
            requestContent.Add(new StringContent(request.comment), "app_doc_comment");
            if (method == HttpMethod.Post)
            {
                response = await client.PostAsync(url, requestContent);
            }
            else if (method == HttpMethod.Put)
            {
                response = await client.PutAsync(url, requestContent);
            }

            return response;
        }
    }
}
