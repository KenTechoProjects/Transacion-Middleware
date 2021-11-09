using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Middleware.Service.BAP;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;

namespace Middleware.Service.Fakes
{
    public class FakePaymentManager : IPaymentManager
    {

        public Task<ServiceResponse<IEnumerable<BillerInfo>>> GetBillers()
        {
            var billers = new List<BillerInfo>(new[]
            {
                new BillerInfo
                {
                    BillerCode = "B001",
                    BillerName = "Test Biller"
                }
            });
            var response = new ServiceResponse<IEnumerable<BillerInfo>>(true);
            response.SetPayload(billers);
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<BillerInfo>>> GetTelcos()
        {
            var telcos = new List<BillerInfo>(new[]
{
                new BillerInfo
                {
                    BillerCode = "T001",
                    BillerName = "Test Telco"
                }
            });
            var response = new ServiceResponse<IEnumerable<BillerInfo>>(true);
            response.SetPayload(telcos);
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<ProductInfo>>> GetProducts(string billerCode)
        {
            var response = new ServiceResponse<IEnumerable<ProductInfo>>(true);
            var products = new List<ProductInfo>
            {
                new ProductInfo
                {
                    IsFixedAmount = true,
                    Price = new decimal(1000),
                    ProductCode = "P001",
                    ProductName = "Test Product 1",
                    ValidationSupported = false,
                    RequestParams = new Dictionary<string, string>
                {
                    {"customer_reference", "Customer Reference" }
                }
                },
                new ProductInfo
                {
                    IsFixedAmount = false,
                    ProductCode = "P002",
                    ProductName = "Test Product 2",
                    ValidationSupported = true,
                    RequestParams = new Dictionary<string, string>
                {
                    {"account_id", "Account Id" },
                    {"customer_email", "Email Address" }
                }
                }
            };
            response.SetPayload(products);
            return Task.FromResult(response);
        }


        public Task<BAPResponse> PayBill(BasePaymentRequest request, string reference, string payerPhoneNumber,
            IDictionary<string, string> ProductParameters)
        {
            return Task.FromResult(new BAPResponse { IsSuccessful = true, Status = BAPStatus.SUCCESS });
        }

        public Task<ServiceResponse<PaymentValidationPayload>> Validate(PaymentValidationRequest request, IDictionary<string, string> ProductParameters)
        {
            var response = new ServiceResponse<PaymentValidationPayload>(true);
            var payload = new PaymentValidationPayload();
            var data = new List<PaymentValidationResponse>();
            data.Add(new PaymentValidationResponse
            {
                Name = "customer_name",
                Label = "Customer Name",
                Type = "text",
                Value = "VICTOR OLATUNDE",
                Readonly = true
            });
            payload.Items = data;
            payload.Command = ValidationCommand.complete;
            response.SetPayload(payload);
            return Task.FromResult(response);
        }

        public string GenerateReference()
        {
            return string.Empty;
        }

        public Task<BAPResponse> GetChannelBillers()
        {
            throw new NotImplementedException();
        }

        public Task<BAPResponse> GetBapBillers()
        {
            throw new NotImplementedException();
        }

        public Task<BAPResponse> GetBapProducts(string slug)
        {
            throw new NotImplementedException();
        }

        public Task<BAPResponse> GetBapIProducts(string slug)
        {
            throw new NotImplementedException();
        }

        public Task<BAPResponse> GetAirtimeBillers()
        {
            throw new NotImplementedException();
        }

        public Task<BAPResponse> Wildcard()
        {
            throw new NotImplementedException();
        }
    }
}
