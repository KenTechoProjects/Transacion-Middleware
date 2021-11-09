
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;

namespace Middleware.Service.Fakes
{
    public class FakeBeneficiaryService : IBeneficiaryService
    {
        private readonly IDictionary<string, IList<TransferBeneficiary>> _transferBeneficiaries;
        private readonly IDictionary<string, IList<PaymentBeneficiary>> _paymentBeneficiaries;


        public FakeBeneficiaryService()
        {
            _transferBeneficiaries = new Dictionary<string, IList<TransferBeneficiary>>();
            _paymentBeneficiaries = new Dictionary<string, IList<PaymentBeneficiary>>();
        }

        public Task<BasicResponse> AddPaymentBeneficiary(PaymentBeneficiary beneficiary, string customerID)
        {
            var response = new BasicResponse(false);
            //beneficiary.BillerName = "Test Biller";
            beneficiary.Reference = Guid.NewGuid().ToString();
            IList<PaymentBeneficiary> beneficiaries;
            lock (this)
            {
                var exists = _paymentBeneficiaries.TryGetValue(customerID, out beneficiaries);
                if (!exists)
                {
                    beneficiaries = new List<PaymentBeneficiary>();
                    _paymentBeneficiaries.Add(customerID, beneficiaries);
                }
            }

            beneficiaries.Add(beneficiary);
            _paymentBeneficiaries[customerID] = beneficiaries;
            response.IsSuccessful = true;
            return Task.FromResult(response);
        }


        public Task<BasicResponse> AddTransferBeneficiary(TransferBeneficiary beneficiary, string customerID)
        {
            var response = new BasicResponse(false);
            beneficiary.InstitutionName = "Test Bank";
            beneficiary.Reference = new Random().Next(2, 100).ToString();
            IList<TransferBeneficiary> beneficiaries;
            lock (this)
            {
                var exists = _transferBeneficiaries.TryGetValue(customerID, out beneficiaries);
                if (!exists)
                {
                    beneficiaries = new List<TransferBeneficiary>();
                    _transferBeneficiaries.Add(customerID, beneficiaries);
                }
            }

            beneficiaries.Add(beneficiary);
            _transferBeneficiaries[customerID] = beneficiaries;
            response.IsSuccessful = true;
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<PaymentBeneficiary>>> GetPaymentBeneficiaries(string customerID)
        {
            var exists = _paymentBeneficiaries.TryGetValue(customerID, out var beneficiaries);
            var response = new ServiceResponse<IEnumerable<PaymentBeneficiary>>(true);
            if (!exists)
            {
                beneficiaries = new List<PaymentBeneficiary>();
            }
            response.SetPayload(beneficiaries);
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<TransferBeneficiary>>> GetTransferBeneficiaries(string customerID)
        {
            var exists = _transferBeneficiaries.TryGetValue(customerID, out var beneficiaries);
            var response = new ServiceResponse<IEnumerable<TransferBeneficiary>>(true);
            if (!exists)
            {
                beneficiaries = new List<TransferBeneficiary>();
            }
            response.SetPayload(beneficiaries);
            return Task.FromResult(response);
        }

        public Task<BasicResponse> RemovePaymentBeneficiary(string beneficiaryID, string customerID)
        {
            var response = new BasicResponse(false);
            var exists = _paymentBeneficiaries.TryGetValue(customerID, out var beneficiaries);
            if (!exists)
            {
                response.Error = new ErrorResponse
                {
                    ResponseDescription = "Beneficiary does not exist"
                };
                return Task.FromResult(response);
            }
            var item = beneficiaries.FirstOrDefault(b => b.Reference == beneficiaryID);
            if (item == null)
            {
                response.Error = new ErrorResponse
                {
                    ResponseDescription = "Beneficiary does not exist"
                };
                return Task.FromResult(response);
            }
            beneficiaries.Remove(item);
            response.IsSuccessful = true;
            return Task.FromResult(response);
        }

        public Task<BasicResponse> RemoveTransferBeneficiary(string beneficiaryID, string customerID)
        {
            var response = new BasicResponse(false);
            var exists = _transferBeneficiaries.TryGetValue(customerID, out var beneficiaries);
            if (!exists)
            {
                response.Error = new ErrorResponse
                {
                    ResponseDescription = "Beneficiary does not exist"
                };
                return Task.FromResult(response);
            }
            var item = beneficiaries.FirstOrDefault(b => b.Reference == beneficiaryID);
            if (item == null)
            {
                response.Error = new ErrorResponse
                {
                    ResponseDescription = "Beneficiary does not exist"
                };
                return Task.FromResult(response);
            }
            beneficiaries.Remove(item);
            response.IsSuccessful = true;
            return Task.FromResult(response);
        }
    }
}