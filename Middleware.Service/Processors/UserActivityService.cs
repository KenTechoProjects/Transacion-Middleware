using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Middleware.Core.DAO;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.Model;
using Middleware.Service.Utilities;
using Newtonsoft.Json;

namespace Middleware.Service.Processors
{
    public class UserActivityService : IUserActivityService
    {
        readonly IUserActivityDAO _userActivityDAO;
        private readonly ICustomerDAO _customerDAO;
        private readonly ILogger _logger;

        public UserActivityService(ICustomerDAO customerDAO, IUserActivityDAO userActivityDAO,
            ILoggerFactory logger)
        {
            _userActivityDAO = userActivityDAO;
            _customerDAO = customerDAO;
            _logger = logger.CreateLogger(typeof(UserActivityService));
        }

        public async Task Add(long customerId, string activityType, string activityResult, string resultDescription)
        {
            try
            { _logger.LogInformation(" Inside add method of user activity");
                var userActivity = new UserActivity()
                {
                    Activity = activityType,
                    ActivityDate = DateTime.Now,
                    ActivityResult = activityResult,
                    ResultDescription = resultDescription,
                    CustomerId = customerId
                };
                await _userActivityDAO.Insert(userActivity);
                _logger.LogInformation(" Finished Saving adding user activity");
            }
            catch(Exception e)
            {
                _logger.LogCritical(e, "Error creating activity log for user {0}. Action = {1}. Result = {2}",
                   customerId, activityType, activityResult );
            }
            
        }

        public async Task AddByUsername(string username, string activityType, string actvityResult, string resultDescription)
        {
            //var usernameIsEmail = Util.IsEmail(username);
            _logger.LogInformation("Inside the AddByUsername method of the UserActivityService by username");
            var customer = await _customerDAO.FindByWalletNumber(username);
            if (customer != null)
            {
                var userActivity = new UserActivity()
                {
                    Activity = activityType,
                    ActivityDate = DateTime.Now,
                    ActivityResult = actvityResult,
                    ResultDescription = resultDescription,
                    CustomerId = customer.Id,
                    walletNumber=username
                };
                _logger.LogInformation("Calling  the _userActivityDAO.Insert method Inside the AddByUsername method of theUserActivityService payload:====>{0}",JsonConvert.SerializeObject(userActivity));

                await _userActivityDAO.Insert(userActivity);
                _logger.LogInformation(" Finished Saving user activity by name");
            }
        }
    }
}
