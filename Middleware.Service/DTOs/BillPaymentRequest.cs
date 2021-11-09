using System;
using System.Collections.Generic;
using Middleware.Core.DTO;

namespace Middleware.Service.DTOs
{
    public class BillPaymentRequest : BasePaymentRequest
    {
        public string Pin { get; set; }
    }
}
