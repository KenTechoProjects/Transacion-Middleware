using System;
namespace Middleware.Service.DTOs
{
    public class PhotoUpdateRequest
    {
        public string PhoneNumber { get; set; }
        public string Picture { get; set; }
        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;

            if (string.IsNullOrEmpty(PhoneNumber))
            {
                problemSource = "Phone Number";
                return false;
            }

            if (string.IsNullOrEmpty(Picture))
            {
                problemSource = "Picture";
                return false;
            }


          

            return true;
        }
    }
}
