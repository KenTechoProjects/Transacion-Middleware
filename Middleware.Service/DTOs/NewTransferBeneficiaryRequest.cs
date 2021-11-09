using System;

namespace Middleware.Service.DTOs
{
    public class NewTransferBeneficiaryRequest
    {
        public TransferBeneficiary Beneficiary { get; set; }
        public Answer Answer { get; set; }

        public bool IsValid(out string problemSource)
        {
            if(!Beneficiary.IsValid(out problemSource))
            {
                return false;
            }
            return true;
        }
    }
}