using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Core.DAO;
using Middleware.Core.Model;
using Middleware.Reversal.Settings;
using Middleware.Service;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Reversal.Services
{
    public   class ReversalService
    {
        private   readonly IReversalDAO _reversalDAO;
        private   readonly ILocalAccountService _localService;
        private   readonly IExternalTransferService _externalSrvice;
        private   readonly IReversalLogDAO _reversalLogDAO;
        private   readonly IInstitutionDAO _institutionDAO;
        private   readonly ILogger<ReversalService> _logger;
        private   readonly ReversalSettings _reversalSettings;
        private   readonly SystemSettings _systemSettings;
        private   readonly Core.DAO.ITransactionDAO _transactionDAO;
        private   readonly IWalletService _walletService;

        public   ReversalService(IReversalDAO reversalDAO, ILocalAccountService localService, IExternalTransferService externalSrvice, IReversalLogDAO reversalLogDAO,
                                        IInstitutionDAO institutionDAO, ILogger<ReversalService> logger, IOptions<ReversalSettings> reversalSettings,
                                         IOptions<SystemSettings> systemSettings, Core.DAO.ITransactionDAO transactionDAO, IWalletService walletService)
        {
            _reversalDAO = reversalDAO;
            _localService = localService;
            _externalSrvice = externalSrvice;
            _reversalLogDAO = reversalLogDAO;
            _institutionDAO = institutionDAO;
            _logger = logger;
            _reversalSettings = reversalSettings.Value;
            _systemSettings = systemSettings.Value;
            _transactionDAO = transactionDAO;
            _walletService = walletService;
        }
        

        public   async Task ReverseAccountTransactions()
        {
            //_logger.LogInformation($"Starting Account-based reversals....");

            var pendingReversals = await _reversalDAO.FindByTypeAndStatus(ReversalType.Account, ReversalStatus.Pending);

            if (!pendingReversals.Any())
            {
                //_logger.LogInformation($"No pending reversals...");
                return;
            }

            foreach (var item in pendingReversals)
            {
                item.ReversalStatus = ReversalStatus.InProcess;
                item.RetryCount++;
                try
                {
                    await _reversalDAO.Update(item);
                    //_logger.LogInformation($"Updating {item.TransactionId} => {item.ReversalStatus}");

                }
                catch (Exception e)
                {
                    _logger.LogError($"Error while processing transaction ID => {item.TransactionId} \\ ERROR: {e}");

                    continue;
                }

                var transaction = item.Transaction;


                var request = new BaseTransferRequest();
                try
                {

                    request.Amount = transaction.Amount;
                    request.DestinationAccountId = transaction.SourceAccountId; //swap source and destination accounts
                    request.SourceAccountId = _systemSettings.WalletFundingAccount?.AccountNumber; // Review the account to put
                    request.DestinationInstitutionId = transaction.DestinationInstitution;
                    request.Narration = $"RVSL-{transaction.Narration}";

                    item.ReversalReference = $"RVSL-{transaction.TransactionReference}";

                    var reversalResponse = await _localService.Transfer(request, item.ReversalReference);
                    //_logger.LogInformation($"Reversal response for {item.TransactionId} => {Newtonsoft.Json.JsonConvert.SerializeObject(reversalResponse)}");

                    if (reversalResponse.IsSuccessful)
                    {
                        item.ReversalStatus = ReversalStatus.Complete;
                        item.StatusMessage = "Successful";
                    }
                    else
                    {
                        item.StatusMessage = reversalResponse.Error?.ResponseDescription;

                        if (item.RetryCount >= _reversalSettings.MaxRetry)
                        {
                            item.ReversalStatus = ReversalStatus.Failed;
                        }
                        else
                        {
                            item.ReversalStatus = ReversalStatus.Pending;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Could not complete revesal for ID => {item.TransactionId} \\ ERROR: {ex}");
                }
                finally
                {
                    try
                    {
                       // item.RetryCount++;
                        item.LastTryDate = DateTime.Now;
                        await _reversalDAO.Update(item);
                        await _reversalLogDAO.Add(new ReversalLog
                        {
                            ReversalReference = item.ReversalReference,
                            ReversalStatus = item.ReversalStatus,
                            ReversalType = item.ReversalType,
                            StatusMessage = item.StatusMessage,
                            TransactionId = item.TransactionId,
                            DestinationAccountID = transaction.SourceAccountId,
                            SourceAccountID = transaction.DestinationAccountID,
                            Amount = transaction.Amount,
                            Narration = request.Narration
                        });


                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Could not update revesal for ID => {item.TransactionId} \\ ERROR: {e}");
                    }


                }



            }

        }
        public   async Task Rev() => await this. ReverseWalletTransactions();
        public   async Task ReverseWalletTransactions()
        {
            //_logger.LogInformation($"Starting Wallet-based reversals....");

            var pendingReversals = await _reversalDAO.FindByTypeAndStatus(ReversalType.Wallet, ReversalStatus.Pending);

            foreach (var item in pendingReversals)
            {
                item.ReversalStatus = ReversalStatus.InProcess;
                try
                {
                    await _reversalDAO.Update(item);
                    //_logger.LogInformation($"Updating {item.TransactionId} => {item.ReversalStatus.ToString()}");

                }
                catch (Exception e)
                {
                    _logger.LogError($"Error while processing transaction ID => {item.TransactionId} \\ ERROR: {e}");

                    continue;
                }

                var transaction = item.Transaction;
                item.RetryCount++;

                item.ReversalReference = DateTime.Now.Ticks.ToString();
                try
                {
                    var reversalResponse = await _walletService.ReverseTransaction(transaction.TransactionReference, transaction.SourceAccountId);

                    if (reversalResponse.IsSuccessful)
                    {
                        item.ReversalStatus = ReversalStatus.Complete;
                    }
                    else
                    {
                        item.StatusMessage = reversalResponse.Error?.ResponseDescription;

                        if (item.RetryCount >= _reversalSettings.MaxRetry)
                        {
                            item.ReversalStatus = ReversalStatus.Failed;
                        }
                        else
                        {
                            item.ReversalStatus = ReversalStatus.Pending;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Could not complete revesal for ID => {item.TransactionId} \\ ERROR: {ex}");
                }
                finally
                {
                    try
                    {
                        
                        item.LastTryDate = DateTime.Now;
                        await _reversalDAO.Update(item);
                        await _reversalLogDAO.Add(new ReversalLog
                        {
                            ReversalReference = item.ReversalReference,
                            ReversalStatus = item.ReversalStatus,
                            ReversalType = item.ReversalType,
                            StatusMessage = item.StatusMessage,
                            TransactionId = item.TransactionId,
                            DestinationAccountID = transaction.SourceAccountId,
                            Amount = transaction.Amount
                        });


                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Could not update revesal for ID => {item.TransactionId} \\ ERROR: {e}");
                    }


                }
            }
        }
    }
}
