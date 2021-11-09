using Middleware.Core.DAO;
using Middleware.Service.DAO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.Fakes
{
    public class FakeUnitOfWorkSession : IUnitOfWorkSession
    {
        public void Commit()
        {
            //do nothing
        }

        public void Rollback()
        {
            //do nothing
        }
    }
}
