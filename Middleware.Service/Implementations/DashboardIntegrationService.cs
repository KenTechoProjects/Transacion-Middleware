using Microsoft.Extensions.Logging;
using Middleware.Core.DAO;
using Middleware.Core.DTO;
using Middleware.Core.Model;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.Implementations
{
    public class DashboardIntegrationService : IDashboardIntegrationService
    {
        readonly ICustomerDAO _customerDAO;
        readonly IMessageProvider _messageProvider;
        readonly ILogger _logger;
        readonly ITransactionDAO _transactionDAO;
        readonly IUserActivityDAO _userActivityDAO;
        readonly IDeviceDAO _deviceDAO;
        readonly ISessionDAO _sessionDAO;
        readonly IDeviceHistoryDAO _deviceHistoryDAO;
        public DashboardIntegrationService(ICustomerDAO customerDAO, IMessageProvider messageProvider, ISessionDAO sessionDAO,
            IDeviceHistoryDAO deviceHistoryDAO,
            ILoggerFactory logger, IUserActivityDAO userActivityDAO, ITransactionDAO transactionDAO, IDeviceDAO deviceDAO)
        {
            _messageProvider = messageProvider;
            _customerDAO = customerDAO;
            _logger = logger.CreateLogger(typeof(DashboardIntegrationService));
            _userActivityDAO = userActivityDAO;
            _transactionDAO = transactionDAO;
            _deviceDAO = deviceDAO;
            _sessionDAO = sessionDAO;
            _deviceHistoryDAO = deviceHistoryDAO;
        }
        public async Task<ServiceResponse<CustomerDetails>> GetCustomerInformaton(string walletNo, string langCode)
        {
            var result = new ServiceResponse<CustomerDetails>(false);
            // Get Customer detail
            var customer = await _customerDAO.FindByWalletNumber(walletNo);
            if (customer == null)
            {
                return ErrorResponse.Create<ServiceResponse<CustomerDetails>>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                    ResponseCodes.CUSTOMER_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, langCode));
            }
            // Get Customer transactions
            var transactions = await _transactionDAO.GetTransactions(customer.Id);

            // Get Customer Activities
            var userActivities = await _userActivityDAO.GetUserActivities(customer.Id);
            var response = new CustomerDetails
            {
                AccountNumber = customer.AccountNumber,
                // c = customer.,
                IsActive = customer.IsActive,
                FirstName = customer.FirstName,
                MiddleName = customer.MiddleName,
                LastName = customer.LastName,
                EmailAddress = customer.EmailAddress,
                DateCreated = customer.DateCreated,
                OnboardingStatus = customer.OnboardingStatus,
                LastLogin = customer.LastLogin,
                HasAccount = customer.HasAccount,
                HasWallet = customer.HasWallet,
                PhoneNumber = customer.PhoneNumber,
                WalletNumber = customer.WalletNumber,
                ReferralCode = customer.ReferralCode,
                ReferredBy = customer.ReferredBy,
                Gender = customer.Gender,
                Title = customer.Title,
                BankId = customer.BankId,
                Transactions = transactions!=null&& transactions.Count()>0? transactions.Select(x => new Transaction
                {
                    CustomerId = x.CustomerId,
                    DestinationAccountID = x.DestinationAccountID,
                    SourceAccountId = x.SourceAccountId,
                    BillerID = x.BillerID,
                    Narration = x.Narration,
                    TransactionReference = x.TransactionReference,
                    DestinationInstitution = x.DestinationInstitution,
                    DestinationCountryId = x.DestinationCountryId,
                    Amount = x.Amount,
                    TransactionType = x.TransactionType,
                    TransactionStatus = x.TransactionStatus,
                    DateCreated = x.DateCreated,
                    ResponseTime = x.ResponseTime,
                    ResponseCode = x.ResponseCode,
                    ProviderReference = x.ProviderReference
                }):null,
                UserActivities = userActivities!=null&& userActivities.Count()>0? userActivities.Select(x => new UserActivity
                {
                    CustomerId = x.CustomerId,
                    Activity = x.Activity,
                    ActivityDate = x.ActivityDate,
                    ActivityResult = x.ActivityResult,
                    ResultDescription = x.ResultDescription
                }):null
            };
            result.SetPayload(response);

            result.IsSuccessful = true;

            return result;
        }
        public async Task<BasicResponse> ActivateDevice(string deviceId, string language)
        {
            var response = new BasicResponse(false);

            var device = await _deviceDAO.Find(deviceId);
            if (device == null)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.CLIENT_INVALID_ARGUMENT, ResponseCodes.DEVICE_NOT_FOUND,
                                                           _messageProvider.GetMessage(ResponseCodes.DEVICE_NOT_FOUND, language));
            }

            if (device.Customer == null)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.CLIENT_INVALID_ARGUMENT, ResponseCodes.CUSTOMER_NOT_FOUND,
                                                           _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language));
            }

            if (device.IsActive)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.INVALID_OBJECT_STATE, ResponseCodes.DEVICE_DISABLED,
                                                               _messageProvider.GetMessage(ResponseCodes.DEVICE_DISABLED, language));
            }

            device.IsActive = true;
            await _deviceDAO.Update(device);
            response.IsSuccessful = true;

            return response;
        }
        public async Task<BasicResponse> DeactivateDevice(string deviceId, string language)
        {
            var response = new BasicResponse(false);

            var device = await _deviceDAO.Find(deviceId);
            if (device == null)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.CLIENT_INVALID_ARGUMENT, ResponseCodes.DEVICE_NOT_FOUND,
                                                           _messageProvider.GetMessage(ResponseCodes.DEVICE_NOT_FOUND, language));
            }
            if (device.Customer == null)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.CLIENT_INVALID_ARGUMENT, ResponseCodes.CUSTOMER_NOT_FOUND,
                                                           _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language));
            }

            if (!device.IsActive)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.INVALID_OBJECT_STATE, ResponseCodes.DEVICE_DISABLED,
                                                               _messageProvider.GetMessage(ResponseCodes.DEVICE_DISABLED, language));
            }
            var ticket = _deviceDAO.Begin();

            _sessionDAO.Join(ticket);
            await _sessionDAO.DeleteCustomerSessions(device.Customer_Id.GetValueOrDefault());
            device.IsActive = false;
            await _deviceDAO.Update(device);
            ticket.Commit();
            response.IsSuccessful = true;

            return response;
        }
        public async Task<ServiceResponse<IEnumerable<CustomerDevice>>> GetCustomerDevices(string walletNo, string langCode)
        {
            var result = new ServiceResponse<IEnumerable<CustomerDevice>>(false);
            var customer = await _customerDAO.FindByWalletNumber(walletNo);
            if (customer == null)
            {
                return ErrorResponse.Create<ServiceResponse<IEnumerable<CustomerDevice>>>(
                    FaultMode.REQUESTED_ENTITY_NOT_FOUND, ResponseCodes.CUSTOMER_NOT_FOUND,
                    _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, langCode));
            }
            var devices = await _deviceDAO.FindByCustomerId(customer.Id);
            var custDevice = devices!=null? devices.Select(x => new CustomerDevice
            {
                DateCreated = x.DateCreated,
                DeviceId = x.DeviceId,
                //Id = x.Id,
                IsActive = x.IsActive,
                Model = x.Model
            }):null;
            result.IsSuccessful = true;
            result.SetPayload(custDevice);

            return result;
        }
        public async Task<BasicResponse> LockProfile(string walletNo, string langCode)
        {
            var response = new BasicResponse(false);
            // Get Customer detail
            var customer = await _customerDAO.FindByWalletNumber(walletNo);
            if (customer == null)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                    ResponseCodes.CUSTOMER_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, langCode));
            }
            if (!customer.IsActive)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.INVALID_OBJECT_STATE, ResponseCodes.INVALID_ENTITY_STATE,
                                                               _messageProvider.GetMessage(ResponseCodes.INVALID_ENTITY_STATE, langCode));
            }
            customer.IsActive = false;
            var ticket = _customerDAO.Begin();

            _sessionDAO.Join(ticket);

            await _customerDAO.UpdateCustomerStatus(customer);

            await _sessionDAO.DeleteCustomerSessions(customer.Id);
            ticket.Commit();

            response.IsSuccessful = true;

            return response;
        }
        public async Task<BasicResponse> ReleaseDevice(string deviceId, string language)
        {
            var response = new BasicResponse(false);

            var device = await _deviceDAO.Find(deviceId);
            if (device == null)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.CLIENT_INVALID_ARGUMENT, ResponseCodes.DEVICE_NOT_FOUND,
                                                           _messageProvider.GetMessage(ResponseCodes.DEVICE_NOT_FOUND, language));
            }

            if (device.Customer == null)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.CLIENT_INVALID_ARGUMENT, ResponseCodes.CUSTOMER_NOT_FOUND,
                                                           _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language));
            }

            var deviceHistory = new Device
            {
                DeviceId = device.DeviceId,
                Model = device.Model,
                Customer_Id = device.Customer_Id,
                IsActive = false,
                DateCreated = DateTime.Now
            };
            await _deviceHistoryDAO.Add(deviceHistory);

            device.Customer_Id = null;
            device.IsActive = false;
            await _deviceDAO.Update(device);

            response.IsSuccessful = true;

            return response;
        }
        public async Task<BasicResponse> UnlockProfile(string walletNo, string langCode)
        {
            var response = new BasicResponse(false);
            // Get Customer detail
            var customer = await _customerDAO.FindByWalletNumber(walletNo);
            if (customer == null)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.REQUESTED_ENTITY_NOT_FOUND,
                    ResponseCodes.CUSTOMER_NOT_FOUND, _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, langCode));
            }
            if (customer.IsActive)
            {
                return ErrorResponse.Create<BasicResponse>(FaultMode.INVALID_OBJECT_STATE, ResponseCodes.INVALID_ENTITY_STATE,
                                                               _messageProvider.GetMessage(ResponseCodes.INVALID_ENTITY_STATE, langCode));
            }
            customer.IsActive = true;
            //var ticket = _customerDAO.Begin();

            //_sessionDAO.Join(ticket);

            await _customerDAO.UpdateCustomerStatus(customer);

            //await _sessionDAO.DeleteCustomerSessions(customer.Id);
            //ticket.Commit();

            response.IsSuccessful = true;

            return response;
        }


    }
}
