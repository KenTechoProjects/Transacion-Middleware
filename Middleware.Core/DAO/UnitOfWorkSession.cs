using System;
using System.Data;

namespace Middleware.Core.DAO
{
    public class UnitOfWorkSession : IUnitOfWorkSession
    {
        readonly IDbTransaction _transaction;
        public UnitOfWorkSession(IDbConnection connection)
        {
            _transaction = connection.BeginTransaction();
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public IDbTransaction GetTransaction()
        {
            return _transaction;
        }
    }
}
