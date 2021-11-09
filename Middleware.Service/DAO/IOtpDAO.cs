using System;
using System.Threading.Tasks;
using Middleware.Core.DAO;
using Middleware.Service.Model;

namespace Middleware.Service.DAO
{
    public interface IOtpDAO : ITransactionCoordinator
    {
        Task Add(Otp otp);
        Task Delete(long id);
        Task<Otp> Find(string userId, OtpPurpose purpose);
        Task Update(Otp otp);
    }
}
