using Middleware.Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Middleware.Service
{
    public interface IBillsService
    {

        Task<ServiceResponse<IEnumerable<BillerInfo>>> GetBillers(string language);
        Task<ServiceResponse<IEnumerable<BillerInfo>>> GetTelcos(string language);
        Task<ServiceResponse<IEnumerable<ProductInfo>>> GetProducts(string billerCode, string language);
        Task<ServiceResponse<PaymentResponse>> BuyAirtime(AirtimePurchaseRequest request, AuthenticatedUser user, bool saveBeneficiary, string language);
        Task<ServiceResponse<PaymentValidationPayload>> Validate(PaymentValidationRequest request, string language);
        Task<ServiceResponse<PaymentResponse>> PayBill(BillPaymentRequest request, AuthenticatedUser user, bool saveBeneficiary, string language);
        Task<ServiceResponse<PaymentResponse>> GetChannelBillers();
        Task<ServiceResponse<PaymentResponse>> GetBapBillers();
        Task<ServiceResponse<PaymentResponse>> GetBapProducts(string slug);
        Task<ServiceResponse<PaymentResponse>> GetBapIProducts(string slug);
        Task<ServiceResponse<PaymentResponse>> GetAirtimeBillers();
        Task<ServiceResponse<PaymentResponse>> Wildcard();
        Task<BasicResponse> SaveAirtimeBeneficiary(AirTimeBenefitiary request, PaymentType type, string walletNumber,string language, string countryId);
        Task<ServiceResponse> GetPaymentBeneficiariesAsyc(string walletNumber, string countryId, string language);
        Task<BasicResponse> DeletePaymentBeneficiariesAsyc(RemovePaymentBeneficiaryPaymentRequest request, AuthenticatedUser user, string language);
    }
}
