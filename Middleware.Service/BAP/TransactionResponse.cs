using System;
using Newtonsoft.Json;

namespace Middleware.Service.BAP
{
    public class TransactionResponse : BaseResponse
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public string Status { get; set; }
        [JsonProperty("reported_status_message")]
        public string Message {get; set; }
        [JsonProperty("reported_status_code")]
        public string PaymentCode {get; set; }
    }
}
