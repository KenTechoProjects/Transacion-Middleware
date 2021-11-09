using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Service.DTOs;

namespace Middleware.Service.Processors
{
    public interface IBeneficiaryService
    {
        Task<ServiceResponse<IEnumerable<TransferBeneficiary>>> GetTransferBeneficiaries(string username);
        Task<BasicResponse> AddTransferBeneficiary(TransferBeneficiary beneficiary, string username);
       
        Task<BasicResponse> RemoveTransferBeneficiary(string beneficiaryID, string username);
        Task<ServiceResponse<IEnumerable<PaymentBeneficiary>>> GetPaymentBeneficiaries(string username);
        Task<BasicResponse> AddPaymentBeneficiary(PaymentBeneficiary beneficiary, string username);
        Task<BasicResponse> RemovePaymentBeneficiary(string beneficiaryID, string username);
    }
}