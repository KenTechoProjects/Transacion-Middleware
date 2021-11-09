using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Middleware.Core.DAO;
using Middleware.Core.DTO;
using Middleware.Core.Model;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Newtonsoft.Json;

namespace Middleware.Service.Processors
{
    public class LimitService : ILimitService
    {
        readonly ILimitDAO _limitDAO;
        readonly ITransactionDAO _transactionDAO;
        private readonly ILogger _log;
        public LimitService(ILimitDAO limitDAO, ITransactionDAO transactionDAO, ILoggerFactory log)
        {
            _limitDAO = limitDAO;
            _transactionDAO = transactionDAO;
            _log = log.CreateLogger(typeof(LimitService));
        }


        public async Task<TransactionLimitResponse> ValidateLimit(long customerId, TransactionType transactionType, decimal amount)
        {
            var response = new TransactionLimitResponse();
            try
            {


                _log.LogInformation(("Inside the ValidateLimit of LimitService"));

                var limit = await _limitDAO.Find(transactionType);
                response.Limit = limit;
                _log.LogInformation("limit find result {o}", JsonConvert.SerializeObject(limit));
                if (limit != null)
                {
                    if (amount > limit.SingleLimit)
                    {
                        response.LimitType = "Single";
                        response.IsLimitExceeded = true;
                        _log.LogInformation("Single transfer limit exceeded");
                        return response;
                    }
                    //else if (amount > limit.DailyLimit)
                    //{
                    //    response.LimitType = "Daily"; _log.LogInformation("Daily ransafer limit exceede");
                    //    response.IsLimitExceeded = true;
                    //    return response;
                    //}
                    else
                    {
                        _log.LogInformation("Payload to   _transactionDAO.GetDailySum {0}",
                                           JsonConvert.SerializeObject(new { customerId, transactionType, DateTime.Now }));
                        var totalSumByCustomer =
                            await _transactionDAO.GetDailySum(customerId, transactionType, DateTime.Now);
                        _log.LogInformation("Response from _transactionDAO.GetDailySum {0}",
                            JsonConvert.SerializeObject(totalSumByCustomer));

                        if ((totalSumByCustomer + amount) > limit.DailyLimit)
                        {
                            response.LimitType = "Daily";
                            response.IsLimitExceeded = true;
                            _log.LogInformation("Response from ValidateLimit in the LimitService. Response:====> {0}",
                                JsonConvert.SerializeObject(response));
                            _log.LogInformation("Daily transfer limit exceeded");
                            return response;
                        }
                    }

                }
                else
                {
                    response.NotFound = true;
                    _log.LogInformation("Response from ValidateLimit for not found in the LimitService. Response:====> {0}",
                        JsonConvert.SerializeObject(response));
                    //  response.LimitType = transactionType.ToString();
                    return response;

                }


                response.IsLimitExceeded = false;
                _log.LogInformation("Final Response from ValidateLimit in the LimitService. Response:====> {0}",
                    JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "An Error occurred in the ValidateLimit in the LimitService");
                return response;
            }

        }
        //Not used yet
        public Task<TransactionLimitResponse> ValidateLimitAfterToken(long customerId, TransactionType transactionType, decimal amount, bool lmitExceeded = false)
        {
            throw new NotImplementedException();
        }

        public async Task<LimitResponse> ValidatePaymentLimit(long customerId, TransactionType transactionType, decimal amount)
        {
            var response = new LimitResponse
            {
                SingleLimitExceeded = false,
                DailyLimitExceeded = false
            };

            var limit = await _limitDAO.Find(transactionType);

            if (amount > limit.SingleLimit)
            {
                response.SingleLimitExceeded = true;
            }

            var totalSumByCustomer = await _transactionDAO.GetDailySum(customerId, transactionType, DateTime.Now);

            if ((totalSumByCustomer + amount) > limit.DailyLimit)
            {
                response.DailyLimitExceeded = true;
            }

            return response;
        }
    }
}
