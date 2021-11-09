using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Middleware.Service.FIServices
{
    public class StatementResponse : BaseResponse
    {
        public IList<Record> Transactions { get; set; }
    }


    public class Record
    {
        [JsonProperty(PropertyName = "ValueDate")]
        public string Date { get; set; }
        [JsonProperty(PropertyName = "TranAmount")]
        public decimal Amount { get; set; }
        [JsonProperty(PropertyName = "TranParticular")]
        public string Narration { get; set; }
        [JsonProperty(PropertyName = "TranRemarks")]
        public string  AdditonalNarration { get; set; }

        [JsonProperty(PropertyName = "PartTranType")]
        public string DrCr { get; set; }
        [JsonProperty(PropertyName = "PostedDate")]
        public string PostedDate { get; set; }
        [JsonProperty(PropertyName = "SerialNo")]
        public long Ordinal { get; set; }
    }
}
