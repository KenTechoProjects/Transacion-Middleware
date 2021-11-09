using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Service.Model;

namespace Middleware.Service.DAO
{
    public interface IUserActivityDAO 
    {
       // Task Add(UserActivity userAction);
        Task Insert(UserActivity userActivity);
        Task<IEnumerable<UserActivity>> GetUserActivities(long CustomerID);
    }
}
