using System;
using System.Data;

namespace Middleware.Core.DAO
{
    public abstract class BaseDAO : ITransactionCoordinator
    {
        protected readonly IDbConnection _connection;
        protected UnitOfWorkSession UnitOfWorkSession;

        public BaseDAO(IDbConnection connection)
        {
            _connection = connection;
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        public IUnitOfWorkSession Begin()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            UnitOfWorkSession = new UnitOfWorkSession(_connection);

            return UnitOfWorkSession;
        }

        public void Join(IUnitOfWorkSession token)
        {
            this.UnitOfWorkSession = (UnitOfWorkSession)token;
        }
    }
}
