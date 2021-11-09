using System;
namespace Middleware.Core.DAO
{
    public interface ITransactionCoordinator
    {
        IUnitOfWorkSession Begin();
        void Join(IUnitOfWorkSession token);
    }
}
