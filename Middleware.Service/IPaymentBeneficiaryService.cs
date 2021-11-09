using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service
{
    public interface IPaymentBeneficiaryService
    {
        Task<ServiceResponse<IEnumerable<PaymentBeneficiary>>> GetBeneficiaries(string customerID, string language);
        Task<BasicResponse> DeleteBeneficiary(string beneficiaryID, string customerID, Answer answer, string language);
        Task<BasicResponse> AddBeneficiaryOld(NewPaymentBeneficiaryRequest request, string customerID, string language);
        Task<BasicResponse> AddBeneficiary(NewPaymentBeneficiaryPaymentRequest request,   AuthenticatedUser user, string language, string countryId);
    }
}
