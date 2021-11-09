using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Core.DAO;
using Middleware.Core.Model;
using Middleware.Reversal.Settings;
using Middleware.Service;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.Implementations
{
    public   class ReversalService: IReversalService
    {
        private   readonly IReversalDAO _reversalDAO;
        private   readonly ILocalAccountService _localService;
        private   readonly IExternalTransferService _externalSrvice;
        private   readonly IReversalLogDAO _reversalLogDAO;
        private   readonly IInstitutionDAO _institutionDAO;
        private   readonly ILogger _logger;
        private   readonly ReversalSettings _reversalSettings;
        private   readonly SystemSettings _systemSettings;
        private   readonly Core.DAO.ITransactionDAO _transactionDAO;
        private   readonly IWalletService _walletService;

        public   ReversalService(IReversalDAO reversalDAO, ILocalAccountService localService, IExternalTransferService externalSrvice, IReversalLogDAO reversalLogDAO,
                                        IInstitutionDAO institutionDAO, ILoggerFactory logger, IOptions<ReversalSettings> reversalSettings,
                                         IOptions<SystemSettings> systemSettings, Core.DAO.ITransactionDAO transactionDAO, IWalletService walletService)
        {
            _reversalDAO = reversalDAO;
            _localService = localService;
            _externalSrvice = externalSrvice;
            _reversalLogDAO = reversalLogDAO;
            _institutionDAO = institutionDAO;
            _logger = logger.CreateLogger(typeof(ReversalService));
            _reversalSettings = reversalSettings.Value;
            _systemSettings = systemSettings.Value;
            _transactionDAO = transactionDAO;
            _walletService = walletService;
        }
        

        public   async Task ReverseAccountTransactions()
        {
            //_logger.LogInformation($"Starting Account-based reversals....");
_logger.LogInformation("Inside the ReverseAccountTransactions method of the ReversalService at {0}", DateTime.UtcNow);
            _logger.LogInformation("Calling the _reversalDAO.FindByTypeAndStatus  in the ReverseAccountTransactions method of the ReversalService at {0}", DateTime.UtcNow);
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
                _logger.LogInformation("Iterating on the result from the  _reversalDAO.FindByTypeAndStatus in  the ReverseAccountTransactions method of the ReversalService at {0}", DateTime.UtcNow);
                try
                {
                    _logger.LogInformation("Started updating the _reversalDAO.Update in the ReverseAccountTransactions method of the ReversalService at {0}; Iteration count:===========>{1} Payload=====>{2}", DateTime.UtcNow,  item.RetryCount, JsonConvert.SerializeObject(item));
                    await _reversalDAO.Update(item);
                    //_logger.LogInformation($"Updating {item.TransactionId} => {item.ReversalStatus}");

                }
                catch (Exception e)
                {
                    _logger.LogError(e,"Error while processing transaction ID => {0}", item.TransactionId);

                    continue;
                }

                var transaction = item.Transaction;
                _logger.LogInformation("Finished  iteration in the ReverseAccountTransactions method of the ReversalService at {0}", DateTime.UtcNow);

                var request = new BaseTransferRequest();
                try
                {

                    request.Amount = transaction.Amount;
                    request.DestinationAccountId = transaction.SourceAccountId; //swap source and destination accounts
                    request.SourceAccountId = _systemSettings.WalletFundingAccount?.AccountNumber; // Review the account to put
                    request.DestinationInstitutionId = transaction.DestinationInstitution;
                    request.Narration = $"RVSL-{transaction.Narration}";

                    item.ReversalReference = $"RVSL-{transaction.TransactionReference}";
                    _logger.LogInformation("Starting to transfer on the  _localService.Transfe in the ReverseAccountTransactions method of the ReversalService at {0}: Payload :==========>{1}", DateTime.UtcNow, JsonConvert.SerializeObject(request));
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
                    _logger.LogCritical( ex, "Could not complete revesal for ID => {0}", item.TransactionId);
                }
                finally
                {
                    try
                    {
                        _logger.LogInformation("Inside the final of try catch in the ReverseAccountTransactions method of the ReversalService at {0}", DateTime.UtcNow);
                        // item.RetryCount++;
                        item.LastTryDate = DateTime.Now;
                        _logger.LogInformation("caling _reversalDAO.Update method in the ReverseAccountTransactions method of the ReversalService at {0}", DateTime.UtcNow);
                        await _reversalDAO.Update(item);
                        _logger.LogInformation("Finished Calling _reversalDAO.Update method in the ReverseAccountTransactions method of the ReversalService at {0}", DateTime.UtcNow);

                        _logger.LogInformation("Caling the _reversalLogDAO.Add in the ReverseAccountTransactions method of the ReversalService at {0}", DateTime.UtcNow);

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
                        _logger.LogError(e,"Could not update revesal for ID => {0}", item.TransactionId);
                    }


                }



            }

        }
        public   async Task Rev() => await this. ReverseWalletTransactions();
        public   async Task ReverseWalletTransactions()
        {
            //_logger.LogInformation($"Starting Wallet-based reversals....");
            _logger.LogInformation("Inside the  ReverseWalletTransactions method of the ReversalService at {0}", DateTime.UtcNow);

            _logger.LogInformation("Calling the  _reversalDAO.FindByTypeAndStatus in the ReverseWalletTransactions method of the ReversalService at {0} . Payload :========>{1}", DateTime.UtcNow,JsonConvert.SerializeObject(new { ReversalType.Wallet, ReversalStatus.Pending}));
            var pendingReversals = await _reversalDAO.FindByTypeAndStatus(ReversalType.Wallet, ReversalStatus.Pending);
            _logger.LogInformation("Finished calling the _reversalDAO.FindByTypeAndStatus in  the ReverseWalletTransactions method of the ReversalService at {0} . Response===============>{1}", DateTime.UtcNow,pendingReversals);
            foreach (var item in pendingReversals)
            {
                item.ReversalStatus = ReversalStatus.InProcess;
                try
                {
                    _logger.LogInformation("Calling the _reversalDAO.Update method in the ReverseWalletTransactions method of the ReversalService at {0} . Payload:========>{0}", DateTime.UtcNow,JsonConvert.SerializeObject(item));
                    await _reversalDAO.Update(item);
                    //_logger.LogInformation($"Updating {item.TransactionId} => {item.ReversalStatus.ToString()}");

                }
                catch (Exception e)
                {
                    _logger.LogError(e,"Error while processing transaction ID => {0}", item.TransactionId);

                    continue;
                }

                var transaction = item.Transaction;
                item.RetryCount++;

                item.ReversalReference = DateTime.Now.Ticks.ToString();
                try
                {
                    _logger.LogInformation("Calling the  _walletService.ReverseTransaction  method in the ReverseWalletTransactions method of the ReversalService at {0} Payload===========>{1}", DateTime.UtcNow,JsonConvert.SerializeObject( new { transaction.TransactionReference, transaction.SourceAccountId}));

                    var reversalResponse = await _walletService.ReverseTransaction(transaction.TransactionReference, transaction.SourceAccountId);
                    _logger.LogInformation("Finished Calling the _reversalDAO.Update method in the ReverseWalletTransactions method of the ReversalService at {0}. Response =========>{1}", DateTime.UtcNow,JsonConvert.SerializeObject(reversalResponse));

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
                    _logger.LogError(ex,"Could not complete revesal for ID => {0}", item.TransactionId);
                }
                finally
                {
                    try
                    {
                       _logger.LogInformation("Inside the  try catch final  of the ReverseWalletTransactions method of the ReversalService at {0}", DateTime.UtcNow);

                        item.LastTryDate = DateTime.Now;
                        _logger.LogInformation("Calling the _reversalDAO.Update method in  the  try catch final  of the ReverseWalletTransactions method of the ReversalService at {0}. Payload======>{1}", DateTime.UtcNow,item);

                        await _reversalDAO.Update(item); 
                        _logger.LogInformation("Finished Calling the _reversalDAO.Update method in  the  try catch final  of the ReverseWalletTransactions method of the ReversalService at {0}", DateTime.UtcNow);

                        var reversalLogDAORequest = new ReversalLog
                        {
                            ReversalReference = item.ReversalReference,
                            ReversalStatus = item.ReversalStatus,
                            ReversalType = item.ReversalType,
                            StatusMessage = item.StatusMessage,
                            TransactionId = item.TransactionId,
                            DestinationAccountID = transaction.SourceAccountId,
                            Amount = transaction.Amount
                        }; 

                        _logger.LogInformation("Calling the _reversalLogDAO.Add  method in  the  try catch final  of the ReverseWalletTransactions method of the ReversalService at {0}. Payload======>{1}", DateTime.UtcNow, JsonConvert.SerializeObject(reversalLogDAORequest));

                      
                        await _reversalLogDAO.Add(reversalLogDAORequest);
                        _logger.LogInformation("Finished Calling the _reversalLogDAO.Add  method in  the  try catch final  of the ReverseWalletTransactions method of the ReversalService at {0}. Payload======>{1}", DateTime.UtcNow,JsonConvert.SerializeObject(item));


                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e,"Could not update revesal for ID => {0}", item.TransactionId);
                    }


                }
            }
        }
    }
}
