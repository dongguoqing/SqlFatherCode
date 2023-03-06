namespace Daemon.Repository.EF
{
    using System.ComponentModel;
    using System.Collections.Generic;
    using Daemon.Common;
    using Daemon.Infrustructure.EF;
    using Daemon.Repository.Contract;
    using Daemon.Model;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Daemon.Repository.EF.Enums;
    using Daemon.Common.Helpers;
    public class BlogUserRoleRepository: Repository<BlogUserRole>, IBlogUserRoleRepository
    {
        protected override Expression<Func<BlogUserRole, int>> PrimaryKeyExpression => r=>r.Id;
        public BlogUserRoleRepository(ApiDBContent context) : base(context)
        {

        }
    }
}
