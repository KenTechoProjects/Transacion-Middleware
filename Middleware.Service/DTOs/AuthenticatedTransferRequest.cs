using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Middleware.Service.DTOs
{
     
    public class AuthenticatedTransferRequest :BaseTransferRequest
    {
        [JsonIgnore]
        public string Pin { get; set; }

        public new bool IsValid(out string problemSource)
        {
            if (string.IsNullOrEmpty(Pin))
            {
                problemSource = "pin";
                return false;
            }
            return base.IsValid(out problemSource);
        }
    }
}
