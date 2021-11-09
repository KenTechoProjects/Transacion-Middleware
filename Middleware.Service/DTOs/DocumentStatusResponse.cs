using Middleware.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
   public class DocumentStatusResponse
    {
        public string PhoneNumber { get; set; }
        public int DocType { get; set; }
        //This added after An error occurred for kyc upload Date:18/06/2021
        public DocumentStatus? Status { get; set; }
    }

 

}
