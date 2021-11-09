using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.Fakes
{
    public class FakeBillsService : IBillsService
    {
        readonly IPaymentManager _paymentManager;

        public FakeBillsService(IPaymentManager paymentManager)
        {
            _paymentManager = paymentManager;
        }

        

        public Task<BasicResponse> BuyAirtime(BuyAirtimeRequest request, AuthenticatedUser user, bool saveBeneficiary, string language)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<BillsCustomerInfo>> DoCustomerLookUp(string billerCode, string referenceNumber, string language)
        {
            return await _paymentManager.DoCustomerLookUp(billerCode, referenceNumber, language);
        }

        public async Task<ServiceResponse<IEnumerable<BillerInfo>>> GetBillers(string language)
        {
            var response = new ServiceResponse<IEnumerable<BillerInfo>>(false);
            var billers = new List<BillerInfo>();

            billers.Add(new BillerInfo
            {
                BillerCode = "Biller01",
                BillerName = "DSTV Senegal",
                Category = 1,
                ReferenceName = "Smart card",
                ValidationSupported = true
            });

            billers.Add(new BillerInfo
            {
                BillerCode = "Biller02",
                BillerName = "Senegal Power",
                Category = 1,
                ReferenceName = "Card number",
                ValidationSupported = true
            });
            billers.Add(new BillerInfo
            {
                BillerCode = "Biller03",
                BillerName = "Senegal Water",
                Category = 1,
                ReferenceName = "",
                ValidationSupported = false
            });

            response.SetPayload(billers);
            response.IsSuccessful = true;
            return response;
        }

        public Task<ServiceResponse<IEnumerable<BillerInfo>>> GetBillersThroughProcessor(string language)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<IEnumerable<ProductInfo>>> GetProducts(string billerCode, string language)
        {
            var response = new ServiceResponse<IEnumerable<ProductInfo>>(false);
            var products = new List<ProductInfo>();

            switch (billerCode)
            {
                case "Biller01":
                    products.Add(new ProductInfo
                    {
                        ProductCode = "Product01",
                        ProductName = "TV Lite",
                        Price = 5000,
                        IsFixedAmount = string.Empty
                    });
                    products.Add(new ProductInfo
                    {
                        ProductCode = "Product03",
                        ProductName = "TV Max",
                        Price = 20000,
                        IsFixedAmount = string.Empty
                    });
                    break;

                case "Biller03":
                    products.Add(new ProductInfo
                    {
                        ProductCode = "Product02",
                        ProductName = "Small water",
                        Price = 1000,
                        IsFixedAmount = string.Empty
                    });
                    break;

                case "Biller02":
                    products.Add(new ProductInfo
                    {
                        ProductCode = "Product04",
                        ProductName = "Mini Energy",
                        Price = 7500,
                        IsFixedAmount = string.Empty
                    });
                    break;
            };

            if (products.Count > 0)
            {
                response.SetPayload(products);
                response.IsSuccessful = true;
            }

            return response;
        }

        public Task<ServiceResponse<IEnumerable<ProductInfo>>> GetProductsThroughProcessor(string billerCode, string language)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<IEnumerable<TelcoInfo>>> GetTelcos(string language)
        {
            var response = new ServiceResponse<IEnumerable<TelcoInfo>>(false);
            var telcos = new List<TelcoInfo>();

            telcos.Add(new TelcoInfo
            {
                //TelcoCode = "Telco01",
                //elcoName = "Orange Senegal"
            });
            telcos.Add(new TelcoInfo
            {
                //TelcoCode = "Telco01",
                //TelcoName = "Airtel Senegal"
            });
            telcos.Add(new TelcoInfo
            {
                //TelcoCode = "Telco01",
                //TelcoName = "MTN Senegal"
            });

            response.SetPayload(telcos);
            response.IsSuccessful = true;
            return response;
        }

        public Task<ServiceResponse<IEnumerable<TelcoInfo>>> GetTelcosThroughProcessor(string language)
        {
            throw new NotImplementedException();
        }

        public Task<BasicResponse> PayBills(PayBillsRequest request, AuthenticatedUser user, bool saveBeneficiary, string language)
        {
            throw new NotImplementedException();
        }
    }
}
