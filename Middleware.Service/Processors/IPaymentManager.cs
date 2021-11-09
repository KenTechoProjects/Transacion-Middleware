using Middleware.Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Middleware.Service.Processors
{
    public interface IPaymentManager
    {
        Task<ServiceResponse<IEnumerable<BillerInfo>>> GetBillers();
        Task<ServiceResponse<IEnumerable<BillerInfo>>> GetTelcos();
        Task<ServiceResponse<IEnumerable<ProductInfo>>> GetProducts(string billerCode);
        Task<BAPResponse> PayBill(BasePaymentRequest request, string reference, string payerPhoneNumber, IDictionary<string,string> productParameters);
        Task<ServiceResponse<PaymentValidationPayload>> Validate(PaymentValidationRequest request, IDictionary<string, string> productParameters);
        string GenerateReference();
        Task<BAPResponse> GetChannelBillers();
        Task<BAPResponse> GetBapBillers();
        Task<BAPResponse> GetBapProducts(string slug);
        Task<BAPResponse> GetBapIProducts(string slug);
        Task<BAPResponse> GetAirtimeBillers();
        Task<BAPResponse> Wildcard();
    }
}
