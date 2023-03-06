using System.Linq;
using Daemon.Common;
using Daemon.Infrustructure.EF;
using Daemon.Repository.Contract;
using Daemon.Model;
using System;
using System.Linq.Expressions;
namespace Daemon.Repository.EF
{
    public class {0}Repository : Repository<{0}>, I{0}Repository
    {
        protected override Expression<Func<{0}, int>> PrimaryKeyExpression => r => r.Id;

        public {0}Repository(ApiDBContent context) : base(context)
        {
        }
    }
}
