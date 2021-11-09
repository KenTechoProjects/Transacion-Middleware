using System;
using Newtonsoft.Json;

namespace Middleware.Service.BAP
{
    public class AccountTransaction : BAPTransaction
    {
        [JsonProperty("account_number")]
        public string SourceAccount { get; set; }
        [JsonProperty("payment_method_code")]
        public new readonly string FundSourceCode = "005";
    }
}
