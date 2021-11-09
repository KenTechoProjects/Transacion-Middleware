using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Service.DTOs;

namespace Middleware.Service
{
    public interface ISubsidiaryTransferService
    {
        Task<IEnumerable<Subsidiary>> GetSubsidiaries();
        Task<ServiceResponse<ForexRate>> GetForexRate(string sourceCurrency, string targetCurrency, string language);
        Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber, string subsidiaryID, string language);
        Task<ServiceResponse<CrossBorderTransferCharge>> GetCharges(decimal amount, string language);
        Task<ServiceResponse<TransferResponse>> Transfer(CrossBorderTransferRequest request, string customerID, string language, bool saveAsBeneficiary);

    }
}