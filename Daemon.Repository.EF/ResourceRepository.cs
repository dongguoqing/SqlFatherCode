namespace Daemon.Repository.EF
{
    using Daemon.Infrustructure.EF;
    using Daemon.Repository.Contract;
    using Daemon.Model;
    using System;
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Daemon.Common.Helpers;
    using Daemon.Repository.EF.Enums;
    using Daemon.Infrustructure.Contract;

    public class ResourceRepository : Repository<Resource>, IResourceRepository
    {
        protected override Expression<Func<Resource, int>> PrimaryKeyExpression => r => r.Id;
        private readonly ApiDBContent _context;
        private readonly IUnitOfWork _unitOfWork;
        public ResourceRepository(ApiDBContent context, IUnitOfWork unitOfWork) : base(context)
        {
            _context = context;
            this._unitOfWork = unitOfWork;
        }

        protected override IEnumerable<Resource> IncludeRelationships(IEnumerable<Resource> entities, ApiDBContent context)
        {
            SetChildren(entities);

            SetParentInfo(entities);

            SetResourceTypeName(entities);

            return entities;
        }

        private void SetResourceTypeName(IEnumerable<Resource> entities)
        {
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, ResourceRelationshipEnum.SetResourceTypeName))
            {
                return;
            }
            var resourceTypeIds = entities.Select(r => r.ResourceType).ToList();
            var dictionaryNameList = this._context.BlogDictionary.Where(r => resourceTypeIds.Contains(r.Id)).ToDictionary(r => r.Id, r => r.Value);
            foreach (var entity in entities)
            {
                if (dictionaryNameList.TryGetValue(entity.ResourceType, out string resourceTypeName))
                {
                    entity.ResourceTypeName = resourceTypeName;
                }
            }
        }

        private void SetParentInfo(IEnumerable<Resource> entities)
        {
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, ResourceRelationshipEnum.SetParentInfo))
            {
                return;
            }
            //这里可以查询所有的 因为菜单不会特特别多
            var resource = this._context.Resource.Where(r => r.Superior != 0).AsEnumerable().ToList();
            foreach (var entity in entities)
            {
                var parent = resource.FirstOrDefault(r => r.Id == entity.Superior);
                entity.Pid = parent?.Id;
                entity.PName = parent?.Label;
            }
        }

        private void SetChildren(IEnumerable<Resource> entities)
        {
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, ResourceRelationshipEnum.SetChildren))
            {
                return;
            }
            //这里可以查询所有的 因为菜单不会特特别多
            var resource = this._context.Resource.Where(r => r.Superior != 0).AsEnumerable().ToList();
            foreach (var entity in entities)
            {
                var children = resource.Where(r => r.Superior == entity.Id).ToList();
                children.ForEach(child =>
                {
                    child.Pid = entity.Id;
                    child.PName = entity.Label;
                });
                entity.Children = children;
            }
        }
    }
}
