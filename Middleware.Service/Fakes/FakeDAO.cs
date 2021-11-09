using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Service.Model;
using Middleware.Service.DAO;
using Middleware.Core.Model;
using Middleware.Core.DAO;

namespace Middleware.Service.Fakes
{
    public class FakeDAO : ICustomerDAO, ISessionDAO, IInstitutionDAO, IDeviceDAO, ITransactionCoordinator, IUserActivityDAO, ILimitDAO,
       /* ITransactionDAO,*/ IWalletOpeningRequestDAO, ITransactionTrackerDAO
    {
        readonly ICollection<Customer> _customers;
        ICollection<Session> _sessions;
        readonly ICollection<Institution> _institutions;
        readonly ICollection<Device> _devices;
        readonly ICollection<UserActivity> _userActions;
        readonly ICollection<Transaction> _transactions;
        readonly ICollection<WalletOpeningRequest> _walletRequests;

        public FakeDAO()
        {
            _customers = new List<Customer>();
            _sessions = new List<Session>();
            _institutions = new List<Institution>();
            _devices = new List<Device>();
            _transactions = new List<Transaction>();
            _userActions = new List<UserActivity>();
            _walletRequests = new List<WalletOpeningRequest>();
            Init();
        }

        void Init()
        {
            var customer = new Customer
            {
                Id = 1,
                BankId = "446022003",
                FirstName = "Test",
                LastName = "User",
                IsActive = true,
                EmailAddress = "test@firstbanknigeria.com",
                AccountNumber = "3074291986",
                OnboardingStatus = OnboardingStatus.COMPLETED
            };

            _customers.Add(customer);

            _institutions.Add(
                new Institution
                {
                    Id = 1,
                    InstitutionCode = "001",
                    InstitutionName = "Ecobank",
                    IsEnabled = true,
                    Category = InstitutionCategory.BANK
                }
            );

            _institutions.Add(
                new Institution
                {
                    Id = 2,
                    InstitutionCode = "002",
                    InstitutionName = "Stanbic IBTC Bank",
                    IsEnabled = true,
                    Category = InstitutionCategory.BANK
                }
            );

            _institutions.Add(
                new Institution
                {
                    Id = 3,
                    InstitutionCode = "003",
                    InstitutionName = "MTN Mobile Money",
                    IsEnabled = true,
                    Category = InstitutionCategory.MOBILE_MONEY
                }
            );

            _institutions.Add(
                new Institution
                {
                    Id = 4,
                    InstitutionCode = "004",
                    InstitutionName = "FBNBank",
                    IsEnabled = true,
                    Category = InstitutionCategory.BANK
                }
            );

            _devices.Add(
                new Device
                {
                    Id = 1,
                    Customer = customer,
                    IsActive = true,
                    Model = "Android",
                    DeviceId = "test",
                    Customer_Id = customer.Id
                }
            );

            _transactions.Add(new Transaction
            {
                Amount = 1000m,
                CustomerId = customer.Id,
                DateCreated = DateTime.Now,
                DestinationAccountID = "0987927241313",
                DestinationCountryId = "04",
                DestinationInstitution = "011",
                Narration = "Test Trans",
                SourceAccountId = customer.AccountNumber,
                TransactionType = TransactionType.NationalTransfer,
                ResponseCode = "00",
                TransactionStatus = TransactionStatus.Successful
            });

            _transactions.Add(new Transaction
            {
                Amount = 3000m,
                CustomerId = customer.Id,
                DateCreated = DateTime.Now,
                DestinationAccountID = "000000200313",
                DestinationCountryId = "04",
                DestinationInstitution = "011",
                Narration = "Test Trans 3",
                SourceAccountId = customer.AccountNumber,
                TransactionType = TransactionType.NationalTransfer,
                ResponseCode = "00",
                TransactionStatus = TransactionStatus.Successful
            });

            _userActions.Add(
                new UserActivity
                {
                    Activity = "Logged In",
                    ActivityDate = DateTime.Now,
                    ActivityResult = "Successful",
                    CustomerId = customer.Id,
                    ResultDescription = "Successful"
                }
            );

            _userActions.Add(
               new UserActivity
               {
                   Activity = "Transfered XOF 5000",
                   ActivityDate = DateTime.Now,
                   ActivityResult = "Successful",
                   CustomerId = customer.Id,
                   ResultDescription = "Successful"
               }
           );
        }

        public Task Add(Session session)
        {
            _sessions.Add(session);
            return Task.CompletedTask;
        }

        Task<Session> ISessionDAO.Find(string token)
        {
            var session = _sessions.SingleOrDefault(t => t.Token == token);
            return Task<Session>.FromResult(session);
        }

        public Task Delete(string token)
        {
            var session = _sessions.SingleOrDefault(t => t.Token == token);
            if (token != null)
            {
                _sessions.Remove(session);
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Institution>> FindByCategory(InstitutionCategory category)
        {
            var result = _institutions.Where(i => i.Category == category);
            return Task<IEnumerable<Institution>>.FromResult(result);
        }

        public Task<IEnumerable<Institution>> FindByCategoryAndStatus(InstitutionCategory category, bool status)
        {
            var result = _institutions.Where(i => i.Category == category && i.IsEnabled == status);
            return Task<IEnumerable<Institution>>.FromResult(result);
        }

        public Task<IEnumerable<Institution>> FindByStatus(bool status)
        {
            var result = _institutions.Where(i => i.IsEnabled == status);
            return Task<IEnumerable<Institution>>.FromResult(result);
        }

        public Task<IEnumerable<Institution>> GetAll()
        {
            return Task<IEnumerable<Institution>>.FromResult(_institutions.AsEnumerable());
        }

        public Task<Customer> FindByEmail(string emailAddress)
        {
            var Customer = _customers.SingleOrDefault(c => c.EmailAddress == emailAddress);
            return Task.FromResult<Customer>(Customer);
        }

        public Task<Customer> FindByCustomerId(string customerId)
        {
            var Customer = _customers.SingleOrDefault(c => c.BankId == customerId);
            return Task.FromResult<Customer>(Customer);
        }

        public Task<Customer> FindByAccountNumber(string accountNumber)
        {
            var Customer = _customers.SingleOrDefault(c => c.AccountNumber == accountNumber);
            return Task.FromResult<Customer>(Customer);
        }
        public Task<Device> Find(string deviceKey)
        {
            var device = _devices.FirstOrDefault(d => d.DeviceId == deviceKey);
            return Task.FromResult(device);
        }

        public Task Add(Device device)
        {
            device.Id = _devices.Count + 1;
            _devices.Add(device);
            return Task.CompletedTask;
        }

        public Task<long> Add(Customer customer)
        {
            customer.Id = _customers.Count + 1;
            _customers.Add(customer);
            return Task.FromResult(customer.Id);
        }

        public Task<Customer> Find(long id)
        {
            var Customer = _customers.SingleOrDefault(c => c.Id == id);
            return Task.FromResult<Customer>(Customer);
        }

        public Task Update(Customer customer)
        {
            var value = _customers.SingleOrDefault(c => c.Id == customer.Id);
            value = customer;

            return Task.CompletedTask;
        }

        Task IDeviceDAO.Update(Device device)
        {
            var value = _devices.SingleOrDefault(c => c.Id == device.Id);
            device = value;
            return Task.CompletedTask;
        }

        public Task DeleteCustomerSessions(long customerId)
        {
            _sessions = _sessions.Where(s => s.Customer_Id != customerId).ToList();

            return Task.CompletedTask;

        }

        public IUnitOfWorkSession Begin()
        {
            return new FakeUnitOfWorkSession();
        }

        public void Join(IUnitOfWorkSession token)
        {
            //return 
        }

        public Task<Limit> Find(TransactionType transactionType)
        {
            var limit = new Limit { DailyLimit = 10000000m, ID = 1, SingleLimit = 10000m, TransactionType = transactionType };
            return Task.FromResult(limit);
        }

      
        public Task<decimal> GetDailySum(long customerId, TransactionType transactionType, DateTime date)
        {
            var sum = _transactions.Sum(x => x.Amount);
            return Task.FromResult(sum);
        }

        public Task<long> Add(Transaction transaction)
        {
            _transactions.Add(transaction);
            return Task.FromResult(1L);
        }

        public Task<bool> Update(Transaction transaction)
        {
            return Task.FromResult(true);
        }


        public Task<Institution> FindByInstitutionCode(string institutionCode)
        {
            var result = _institutions.FirstOrDefault(x => x.InstitutionCode == institutionCode);
            return Task.FromResult(result);

        }


        public Task Insert(UserActivity userActivity)
        {
            _userActions.Add(userActivity);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Transaction>> GetTransactions(long customerId)
        {
            return Task.FromResult(_transactions.AsEnumerable());
        }

        public Task UpdateCustomerStatus(Customer customer)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Exists(string walletNumber)
        {
            var exists = _customers.Any(c => c.WalletNumber == walletNumber);
            return Task.FromResult(exists);
        }

        public Task  Add(WalletOpeningRequest entity)
        {
            _walletRequests.Add(entity);
            return Task.CompletedTask;
        }

        Task<WalletOpeningRequest> IWalletOpeningRequestDAO.Find(string walletNumber)
        {
            var request = _walletRequests.FirstOrDefault(w => w.PhoneNumber == walletNumber);
            return Task.FromResult(request);
        }

        public Task Update(WalletOpeningRequest entity)
        {
            var item = _walletRequests.FirstOrDefault(i => i.PhoneNumber == entity.PhoneNumber);
            if (item != null)
            {
                item = entity;
            }
            return Task.CompletedTask;
        }

        public Task<Customer> FindByWalletNumber(string walletNumber)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Device>> FindByCustomerId(long customer_id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserActivity>> GetUserActivities(long CustomerID)
        {
            return Task.FromResult(_userActions.AsEnumerable());
        }

        public Task<bool> IsAvailable(string deviceID)
        {
            return Task.FromResult(true);
        }

        public Task<int> CountAssignedDevices(long customerId)
        {
            return Task.FromResult(2);
        }

        public Task<long> Add(Tracker trackingInfo)
        {
            return Task.FromResult(1L);
        }

        Task<bool> ITransactionTrackerDAO.Find(string transactionReference)
        {
            return Task.FromResult(true);
        }

        public Task DeleteCustomerSessions(string username)
        {
            return Task.CompletedTask;
        }

        Task<long> IWalletOpeningRequestDAO.Add(WalletOpeningRequest entity)
        {
             return Task.FromResult(long.MinValue);
        }

        public Task<bool> CustomerIdIsValid(long customerId)
        {
            throw new NotImplementedException();
        }

        Task<string> IDeviceDAO.Add(Device device)
        {
            throw new NotImplementedException();
        }

        public Task<Device> FindByCustomerIdAndDeviceId(long csutomerId, string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FindByReferralCode(string referrelaCode)
        {
            throw new NotImplementedException();
        }

        Task IUserActivityDAO.Insert(UserActivity userActivity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasReachedLoginFailCount(long customerid)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetMaxLoginCount()
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasReachedLoginFailCount(string walletNumber)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReferralCodeExists(string referralCode)
        {
            throw new NotImplementedException();
        }

        Task<Customer> ICustomerDAO.Add(Customer customer)
        {
            throw new NotImplementedException();
        }





        //public Task<bool> Update(Transaction transaction)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IEnumerable<Transaction>> ITransactionDAO.GetTransactions(long customerId)
        //{
        //    throw new NotImplementedException();
        //}
    }
}