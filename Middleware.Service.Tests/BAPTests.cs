using NUnit.Framework;
using Middleware.Service.BAP;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Middleware.Service.DTOs;
using System.Collections.Generic;
 
using Middleware.Service.Utilities;
using Middleware.Core.DTO;

namespace Middleware.Service.Tests
{
    public class BAPTests
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task TestGetBillers()
        {
            var manager = GetPaymentManager();
            var billers = await manager.GetBillers();
            Assert.IsTrue(billers.IsSuccessful, "Get billers error - {0}",
                new[] { billers.Error });
        }

        [Test]
        public async Task TestGetTelcos()
        {
            var manager = GetPaymentManager();
            var telcos = await manager.GetTelcos();
            Assert.IsTrue(telcos.IsSuccessful, "Get telcos error - {0}",
                new[] { telcos.Error });
        }


        [Test]
        public async Task TestGetProducts()
        {
            var manager = GetPaymentManager();
            var billerCode = "1`wari-saer";
            var products = await manager.GetProducts(billerCode);
            Assert.IsTrue(products.IsSuccessful, "Get products error - {0}",
                new[] { products.Error });
        }

        [Test]
        public async Task TestBillPaymentFromAccount()
        {
            var manager = GetPaymentManager();
            var reference = manager.GenerateReference();
            var request = new BasePaymentRequest
            {
                Amount = 6900,
                BillerCode = "1",
                ProductCode = "wari-saer-compact",
                CustomerReference = "1234567890",
                SourceAccountId = "000000123456789",
                SourceAccountType = AccountType.BANK,
                PaymentParameters = new Dictionary<string, string>(new[]
                {
                    new KeyValuePair<string, string>("email", "a@b.com")
                }
                    )
            };
            var parameters = new Dictionary<string, string>(new[]
                {
                    new KeyValuePair<string, string>("referenceKey", "smart_card_number")
                });

            //var billerCode = "1`wari-saer";
            var rsp = await manager.PayBill(request, reference, "08057700001", parameters);
            Assert.IsTrue(rsp.IsSuccessful, "Bill payment error - {0}",
                new[] { rsp.Error });
        }

        [Test]
        public async Task TestPaymentValidation()
        {
            var manager = GetPaymentManager();
            var request = new PaymentValidationRequest()
            {
                BillerCode = "",
                ProductCode = "",
                CustomerDetails = new Dictionary<string, string>(new[]
                {
                    new KeyValuePair<string, string>("smart_card_number", "1234567890")
                    //new KeyValuePair<string, string>("product_code", "01")
                })
            };
            var parameters = new Dictionary<string, string>(new[]
    {
                    new KeyValuePair<string, string>("validationPath", "service/endpoints/caetano-renting-senegal-sa-smart-card-number-validation-validate-smart-card-number")
                });
            var response = await manager.Validate(request, parameters);
            Assert.IsTrue(response.IsSuccessful, "Payment validation error - {0}",
                new[] { response.Error });
        }

        [Test]
        public async Task TestBuyAirtime()
        {
            var manager = GetPaymentManager();
            var reference = manager.GenerateReference();
            var request = new BasePaymentRequest
            {
                Amount = 500,
                BillerCode = "1",
                ProductCode = "wari-saer-digital-card",
                CustomerReference = "1234567890",
                SourceAccountId = "000000123456789",
                SourceAccountType = AccountType.BANK,
                PaymentParameters = null
            };

            var rsp = await manager.PayBill(request, reference, "08057700001", null);
            Assert.IsTrue(rsp.IsSuccessful, "Airtime purchase error - {0}",
                new[] { rsp.Error });

        }

        private static PaymentManager GetPaymentManager()
        {
            var options = Options.Create(new BillsPaySettings
            {
                AppKey = "bghL9Iy6bZapQWXzxzafnuv4l1RpTMob",
                BaseAddress = "https://10.2.248.79:8445/",
                Endpoints = new BillsPayEndpoints
                {
                    Billers = ""
                },
                TransactionPath = "",
                RequestTimeout = 30
            });
            var client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseDefaultCredentials = true,
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (message, cert, chain, policy) =>
                {
                    return true;
                }
            });

            client.DefaultRequestHeaders.Add("Accept-Language", "en");
              var logger = new LoggerFactory().CreateLogger<PaymentManager>();
            return  new PaymentManager(options, client,(ILoggerFactory)logger, new CodeGenerator());
        }
    }
}