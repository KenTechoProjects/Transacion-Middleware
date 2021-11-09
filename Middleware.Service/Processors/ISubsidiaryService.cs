using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Service.DTOs;

namespace Middleware.Service.Processors
{
    public interface ISubsidiaryService
    {
        Task<IEnumerable<Subsidiary>> GetSubsidiaries();
        Task<ServiceResponse<ForexRate>> GetForexRate(string sourceCurrency, string targetCurrency);
        Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber, string subsidiaryID);
        Task<BasicResponse> Transfer(CrossBorderTransferRequest request, string reference);
        Task<ServiceResponse<CrossBorderTransferCharge>> GetCharges(decimal amount);
    }
}