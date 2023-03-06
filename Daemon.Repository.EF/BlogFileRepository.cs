using System.Linq;
using Daemon.Common;
using Daemon.Infrustructure.EF;
using Daemon.Repository.Contract;
using Daemon.Model;
using System;
using System.Linq.Expressions;
namespace Daemon.Repository.EF
{
    public class BlogFileRepository : Repository<BlogFile>, IBlogFileRepository
    {
        protected override Expression<Func<BlogFile, int>> PrimaryKeyExpression => r => r.Id;

        public BlogFileRepository(ApiDBContent context) : base(context)
        {
        }
    }
}
