using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Daemon.Infrustructure.Contract
{
    public interface IUnitOfWork:IDisposable
    {
        IUnitOfWork BeginTransaction();

        void CommitTransaction();

        void RollbackTransaction();
    }
}
