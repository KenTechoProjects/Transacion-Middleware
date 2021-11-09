using Middleware.Core.DTO;
using System;
namespace Middleware.Service.DTOs
{
    public class IdUpdateRequest
    {
        public string PhoneNumber { get; set; }
        public string IdNumber { get; set; }
        public IdentificationType IdType { get; set; }
        public string Nationality { get; set; }
        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;

            if (string.IsNullOrEmpty(PhoneNumber))
            {
                problemSource = "Phone Number";
                return false;
            }

            if (string.IsNullOrEmpty(IdNumber))
            {
                problemSource = "Id number";
                return false;
            }
 

            if (string.IsNullOrEmpty(Nationality))
            {
                problemSource = "Nationality";
                return false;
            }
 
            return true;
        }

    }
}
