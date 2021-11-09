using System;
using Newtonsoft.Json;

namespace Middleware.Service.BAP
{
    public abstract class BaseResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        [JsonProperty("status_code")]
        public string StatusCode { get; set; }
    }
}
