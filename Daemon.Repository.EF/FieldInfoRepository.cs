namespace Daemon.Repository.EF
{
    using Microsoft.AspNetCore.Http;
    using Daemon.Infrustructure.EF;
    using Daemon.Repository.Contract;
    using Daemon.Model;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using Daemon.Repository.EF.Enums;
    using Daemon.Common.Helpers;

    public class FieldInfoRepository : Repository<FieldInfo>, IFieldInfoRepository
    {
        protected override Expression<Func<FieldInfo, int>> PrimaryKeyExpression => r => r.Id;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public FieldInfoRepository(ApiDBContent context, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override IQueryable<FieldInfo> SelectEntitiesBeforeSelect(IQueryable<FieldInfo> entities, ApiDBContent db)
        {
            entities = GetMyFieldInfo(entities);
            entities = GetCommonTableInfo(entities);
            return entities;
        }

        protected override void UpdateEntitiesBeforeCreate(IEnumerable<FieldInfo> entities, ApiDBContent context)
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(r => r.Type == "UserId")?.Value;
            foreach (var entity in entities)
            {
                entity.CreateTime = DateTime.Now;
                entity.UpdateTime = DateTime.Now;
                entity.UserId = Convert.ToInt32(userId);
            }
        }

        private IQueryable<FieldInfo> GetMyFieldInfo(IQueryable<FieldInfo> entities)
        {
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, FieldInfoRelationshipEnum.MyFieldInfo))
            {
                return entities;
            }

            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(r => r.Type == "UserId")?.Value;
            entities = entities.Where(r => r.UserId == Convert.ToInt32(userId));
            return entities;
        }

        private IQueryable<FieldInfo> GetCommonTableInfo(IQueryable<FieldInfo> entities)
        {
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, FieldInfoRelationshipEnum.Common))
            {
                return entities;
            }

            entities = entities.Where(r => r.IsPublic == 1);
            return entities;
        }
    }
}
