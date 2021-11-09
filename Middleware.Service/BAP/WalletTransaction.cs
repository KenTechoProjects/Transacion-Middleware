using System;
using Newtonsoft.Json;

namespace Middleware.Service.BAP
{
    public class WalletTransaction : BAPTransaction
    {
        [JsonProperty("wallet_id")]
        public string SourceWallet { get; set; }
        [JsonProperty("payment_method_code")]
        public new const string FundSourceCode = "007";
    }
}
