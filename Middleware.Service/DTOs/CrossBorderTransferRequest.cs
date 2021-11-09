using System;

namespace Middleware.Service.DTOs
{
    public class CrossBorderTransferRequest : TransferRequest
    {
        public string DestinationCountry { get; set; }
    }
}