using System;
using Newtonsoft.Json;

namespace Middleware.Service.BAP
{
    public class BillerResponse : BaseResponse
    {
        [JsonProperty("Data")]
        public Category[] Categories { get; set; }
    }

    public class Biller
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Enabled { get; set; }
    }

    public class Category
    {
        public string Name { get; set; }
        public Biller[] Billers { get; set; }
    }
}
