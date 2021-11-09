using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.Utilities
{
    public class ConnectionConfigurations : IConnectionConfigurations
    {
        private readonly IDbConnection _con;
        public ConnectionConfigurations(IDbConnection con)
        {
            _con = con;
        }
        public IDbTransaction Transaction( )
        {
            if (_con.State == ConnectionState.Closed) { _con.Open(); }

           var trans = _con.BeginTransaction();          
            return trans;
        }

    }

    public class ConnectionConfigurationCommitRoll : ConnectionConfigurations, IConnectionConfigurationCommitRoll
    {

        public ConnectionConfigurationCommitRoll(IDbConnection con) : base(con)
        {

        }
        public void Commit()
        {
            Transaction();
        }
        public void RollBack()
        {
            Transaction();
        }

    }

    public class Connections:ConnectionConfigurationCommitRoll,IDisposable
    {
        private readonly IDbConnection _con;
        public Connections(IDbConnection conBase ):base(conBase)
        {
            _con = conBase;
        }

        public void Dispose()
        {
            Transaction().Dispose();
            _con.Close();
        }
    }

}
