using Daemon.Common;
using Daemon.Infrustructure.EF;
using Daemon.Repository.Contract;
using Daemon.Model;
using System;
using System.Linq.Expressions;
namespace Daemon.Repository.EF
{
    public class BlogSysInfoRepository : Repository<BlogSysInfo>, IBlogSysInfoRepository
    {
        protected override Expression<Func<BlogSysInfo, int>> PrimaryKeyExpression => null;
        public BlogSysInfoRepository(ApiDBContent context) : base(context)
        {

        }
    }
}
