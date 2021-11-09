using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Service.DTOs;

namespace Middleware.Service.Processors
{
    public interface ILocalAccountService
    {

        Task<ServiceResponse<IEnumerable<BankAccount>>> GetAccounts(string customerID);
        Task<ServiceResponse<IEnumerable<StatementRecord>>> GetTransactions(string accountNumber, DateTime start, DateTime end);
        Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber);
        Task<BasicResponse> Transfer(BaseTransferRequest request, string reference);
        Task<BasicResponse> TransferToSelf(SelfTransferRequest request, string reference);
        Task<BasicResponse> IsCustomerAccount(string accountNumber, string customerID);
        Task<BasicResponse> DoAccountsBelongtoCustomer(string customerID, string sourceAccount, string destinationAccount);
        Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber, string bankCode);

    
    }
}
