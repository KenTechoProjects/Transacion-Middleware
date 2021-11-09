using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Service.DTOs;

namespace Middleware.Service
{
    public interface ITransferBeneficiaryService
    {
        Task<ServiceResponse<IEnumerable<TransferBeneficiary>>> GetBeneficiaries(AuthenticatedUser user, string language);
        Task<BasicResponse> DeleteBeneficiary(string beneficiaryID, AuthenticatedUser user, Answer answer, string language);
        Task<BasicResponse> AddBeneficiary(NewTransferBeneficiaryRequest request, AuthenticatedUser user, string language);

    }
}