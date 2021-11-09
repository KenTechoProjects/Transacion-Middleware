using Newtonsoft.Json;

namespace Middleware.Service.BAP
{
    public class ProductResponse : BaseResponse
    {
        [JsonProperty("Data")]
        public Envelope Payload { get; set; }
    }

    public class Product
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public decimal Amount { get; set; }
        public Form Form { get; set; }
        [JsonProperty("amount_type")]
        public string AmountType { get; set; }
    }

    public class Envelope
    {
        [JsonProperty("items")]
        public Product[] Products { get; set; }
    }

    public class FormItem
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool Readonly { get; set; }
    }

    public class FormData
    {
        public string Endpoint { get; set; }

        [JsonProperty("payloads")]
        public FormItem[] Parameters { get; set; }
    }

    public class Form
    {
        [JsonProperty("action")]
        public FormData Data { get; set; }
    }
}
