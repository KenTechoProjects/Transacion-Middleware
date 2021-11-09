using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Middleware.Service.BAP
{
    public  abstract class BAPTransaction
    {
        [JsonProperty("biller_id")]
        public string BillerCode { get; set; }
        [JsonProperty("slug")]
        public string ProductCode { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("transaction_reference")]
        public string RequestReference { get; set; }
        [JsonProperty("customer_id")]
        public string CustomerReference { get; set; }
        [JsonProperty("customer_phone")]
        public string CustomerPhoneNumber { get; set; }
        [JsonProperty("type")]
        public readonly string TransactionType = "item";
        [JsonIgnore]
        public readonly string FundSourceCode = "";
        [JsonProperty("payloads")]
        public IList<PayloadItem> PaymentParameters { get; set; }
    }

    public class PayloadItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public enum BAPStatus : byte
    {
        SUCCESS,
        PENDING,
        FAILED,
        REVERSED,
        PENDING_REVERSAL
    }
}
