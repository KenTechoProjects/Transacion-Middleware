using System.Collections.Generic;

namespace Middleware.Service.DTOs
{
    public class PaymentValidationResponse
    {
        // public string AccountName { get; set; }
        // public IDictionary<string, string> TraceParameters { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Readonly { get; set; }

    }

    public class PaymentValidationPayload
    {
        public List<PaymentValidationResponse> Items { get; set; }
        public ValidationCommand Command { get; set; }
        public string ValidationPath { get; set; }
    }

    public enum ValidationCommand
    {
        complete,
        merge
    }
}
