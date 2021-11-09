using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;

namespace Middleware.Service.Fakes
{
    public class FakeSubsidiaryService : ISubsidiaryService
    {
        public Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber, string subsidiaryID)
        {
            var result = new ServiceResponse<dynamic>(true);
            result.SetPayload("FirstBank User");
            return Task.FromResult(result);
        }

        public Task<ServiceResponse<CrossBorderTransferCharge>> GetCharges(decimal amount)
        {
            var charge = new CrossBorderTransferCharge(0, 0, 0);
            var response = new ServiceResponse<CrossBorderTransferCharge>(true);
            response.SetPayload(charge);
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<ForexRate>> GetForexRate(string sourceCurrency, string targetCurrency)
        {
            var result = new ServiceResponse<ForexRate>(true);
            result.SetPayload(new ForexRate
            {
                Target = targetCurrency,
                Source = sourceCurrency,
                Rate = 10
            });
            return Task.FromResult(result);
        }

        public Task<IEnumerable<Subsidiary>> GetSubsidiaries()
        {
            IEnumerable<Subsidiary> result = new List<Subsidiary>
            {
                new Subsidiary
                {
                    SubsidiaryName = "FirstBank Nigeria",
                    CountryID = "01",
                    Currency = "NGN"
                },
                new Subsidiary
                {
                    SubsidiaryName = "FBNBank Ghana",
                    CountryID = "02",
                    Currency = "GHS"
                },
                new Subsidiary
                {
                    SubsidiaryName = "FBNBank Senegal",
                    CountryID = "03",
                    Currency = "XOF"
                },
                new Subsidiary
                {
                    SubsidiaryName = "FBNBank DRC",
                    CountryID = "04",
                    Currency = "CDF"
                }
            };

            return Task<IEnumerable<Subsidiary>>.FromResult(result);
        }

        public Task<BasicResponse> Transfer(CrossBorderTransferRequest request, string reference)
        {
            var response = new BasicResponse(true);
            
            return Task.FromResult(response);
        }
    }
}