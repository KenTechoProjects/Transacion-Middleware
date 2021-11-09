using System;
namespace Middleware.Core.DAO
{
    public interface IUnitOfWorkSession
    {
        void Commit();
        void Rollback();
    }
}
