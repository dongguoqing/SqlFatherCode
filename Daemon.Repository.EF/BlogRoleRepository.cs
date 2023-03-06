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
    public class BlogRoleRepository : Repository<BlogRole>, IBlogRoleRepository
    {
        protected override Expression<Func<BlogRole, int>> PrimaryKeyExpression => r => r.Id;
        private readonly BlogRoleResourceRepository _blogRoleResourceRepository;
        public BlogRoleRepository(ApiDBContent context, BlogRoleResourceRepository blogRoleResourceRepository) : base(context)
        {
            _blogRoleResourceRepository = blogRoleResourceRepository;
        }

        protected override IEnumerable<BlogRole> IncludeRelationships(IEnumerable<BlogRole> entities, ApiDBContent context)
        {
            SetResourceList(entities);

            return entities;
        }

        protected override void UpdateEntitiesAfterUpdate(IEnumerable<BlogRole> entities, ApiDBContent context)
        {
            UpdateRoleResource(entities);
        }

        private void UpdateRoleResource(IEnumerable<BlogRole> entities)
        {
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, BlogRoleRelationshipEnum.UpdateRoleResource))
            {
                return;
            }
            var roleResourceList = new List<BlogRoleResource>();
            var deleteRoleIds = new List<int>();
            foreach (var entity in entities)
            {
                var idList = entity.IdList;
                if (entity.ResourceList != null)
                {
                    deleteRoleIds.AddRange(entity?.ResourceList?.Select(r => r.Id));
                }
                idList.ForEach(r =>
                {
                    var roleResource = new BlogRoleResource()
                    {
                        RoleId = entity.Id,
                        ResourceId = r
                    };
                    roleResourceList.Add(roleResource);
                });
            }

            _blogRoleResourceRepository.DeleteRangeByIds(deleteRoleIds);
            _blogRoleResourceRepository.AddRange(roleResourceList);
        }

        private void SetResourceList(IEnumerable<BlogRole> entities)
        {
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, BlogRoleRelationshipEnum.SetResourceList))
            {
                return;
            }

            var roleIds = entities.Select(r => r.Id).ToList();
            var blogRoleResource = this.Context.BlogRoleResource.AsNoTracking().Where(a => roleIds.Contains(a.RoleId)).ToList();
            var resourceList = (from a in blogRoleResource
                                join b in Context.Resource on a.ResourceId equals b.Id
                                select b).ToList();
            foreach (var entity in entities)
            {
                var roleResource = blogRoleResource.Where(r => r.RoleId == entity.Id).ToList();
                roleResource.ForEach(r =>
                {
                    r.Resource = resourceList.FirstOrDefault(resource => resource.Id == r.ResourceId);
                });
                entity.ResourceList = roleResource;
            }
        }
    }
}
