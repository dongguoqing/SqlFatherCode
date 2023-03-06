using System.Linq;
using Daemon.Common;
using Daemon.Infrustructure.EF;
using Daemon.Repository.Contract;
using Daemon.Model;
using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Daemon.Common.Helpers;
using Daemon.Repository.EF.Enums;
using System.Collections.Generic;
namespace Daemon.Repository.EF
{
    public class BlogRoleResourceRepository : Repository<BlogRoleResource>, IBlogRoleResourceRepository
    {
        protected override Expression<Func<BlogRoleResource, int>> PrimaryKeyExpression => r => r.Id;
        public BlogRoleResourceRepository(ApiDBContent context) : base(context)
        {

        }

        protected override IEnumerable<BlogRoleResource> IncludeRelationships(IEnumerable<BlogRoleResource> entities, ApiDBContent context)
        {
            return entities;
        }

        protected override void UpdateEntitiesAfterCreate(IEnumerable<BlogRoleResource> entities, ApiDBContent context)
        {
        }
    }
}
