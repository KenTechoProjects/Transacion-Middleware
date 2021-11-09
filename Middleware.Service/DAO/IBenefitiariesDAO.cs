using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.DAO
{
    public interface IBenefitiariesDAO
    {

        Task<BasicResponse> SaveAirtimeBeneficiary(AirTimeBenefitiary request, string language);
        Task<IEnumerable<AirTimeBenefitiaryResponse>> GetPaymentBeneficiariesAsyc(string walletNumber, string countryId);
        Task<BasicResponse> DeletePaymentBeneficiariesAsyc(long id, string walletNumber, string customerId, string language); 

    Task<Dictionary<bool,string>>  BeneficiaryExistssAsyc(string walletNumber,   string beneficiaryNumber, string countryId, string referenceNumber, PaymentType paymentType); 

    }
}
