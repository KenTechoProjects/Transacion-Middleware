using Middleware.Service.DTOs;
using Middleware.Service.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.Processors
{
    public interface IUserActivityService
    {
        Task Add(long customerId, string activityType, string actvityResult, string resultDescription);
        Task AddByUsername(string username, string activityType, string actvityResult, string resultDescription);
     }
}
