using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.Processors
{
    public class TransactionTracker : ITransactionTracker
    {
        readonly ITransactionTrackerDAO _transactionTrackerDAO;

        public TransactionTracker(ITransactionTrackerDAO transactionTrackerDAO)
        {
            _transactionTrackerDAO = transactionTrackerDAO;
        }

        public async Task<BasicResponse> AddTransactionReference(long customerId, string transRef)
        {
            var response = new BasicResponse(false);

            //if (!await Exists(transRef))
            //{
            //    return response;
            //}

            var tracker = new Tracker
            {
                CustomerId = customerId,
                TransactionReference = transRef,
                TransactionTime = DateTime.Now
            };
            var result = await _transactionTrackerDAO.Add(tracker);
            if (result > 0)
            {
                response.IsSuccessful = true;
            }
            return response;
        }

        //public async Task<bool> Exists(string transReference)
        //{
        //    return await _transactionTrackerDAO.Find(transReference);
        //}

       public async Task<bool> Exists(string transRef)
        {
            var response = false;
            var exist = await _transactionTrackerDAO.Find(transRef);

            if (exist)
            {
                response = true;
            }

            return response;
        }
    }
}
