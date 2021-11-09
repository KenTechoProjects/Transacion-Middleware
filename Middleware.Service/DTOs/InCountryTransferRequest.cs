using System;

namespace Middleware.Service.DTOs
{
    public class InCountryTransferRequest : TransferRequest
    {
        public string DestinationInstitution { get; set; }
        public string DestinationBranchCode { get; set; }
    }
}