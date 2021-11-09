using Middleware.Service.FIServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class AccountOpeningResponse : BaseResponse
    {
        public string AccountNumber { get; set; }
    }
}
