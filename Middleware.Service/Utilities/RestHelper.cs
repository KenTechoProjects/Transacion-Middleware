using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.Utilities
{
    public class RestHelper : IRestHelper
    {
        private readonly string _baseUrl;
        private readonly RestClient _client;
        readonly string _appId;
        readonly string _appKey;

        public RestHelper(string baseUrl, string appId, string appKey)
        {
            _baseUrl = baseUrl;
            _client = new RestClient(_baseUrl);
            _appId = appId;
            _appKey = appKey;
        }

        public IRestResponse Get<T>(string endpoint, IDictionary<string, T> values = null)
        {

            var request = new RestRequest(endpoint, Method.GET);
            request.AddHeader("AppId", _appId);
            request.AddHeader("AppKey", _appKey);
            if (values != null)
            {
                foreach (var itm in values)
                {
                    request.AddParameter(itm.Key, itm.Value);
                }
            }

            IRestResponse response = _client.Execute(request);
            return response;
        }

        public IRestResponse POST<T>(string endpoint, T value) 
        {
            var request = new RestRequest(endpoint, Method.POST);
            request.AddHeader("AppId", _appId);
            request.AddHeader("AppKey", _appKey);
            request.AddJsonBody(value);
            
            IRestResponse response = _client.Execute(request);
           
            return response;
        }

        public IRestResponse PUT<T>(string endpoint, T value)
        {
            var request = new RestRequest(endpoint, Method.PUT);
            request.AddHeader("AppId", _appId);
            request.AddHeader("AppKey", _appKey);
            request.AddJsonBody(value);

            IRestResponse response = _client.Execute(request);

            return response;
        }
    }
}
