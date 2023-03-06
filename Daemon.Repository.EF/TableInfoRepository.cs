namespace Daemon.Repository.EF
{
    using Daemon.Infrustructure.EF;
    using Daemon.Repository.Contract;
    using Daemon.Model;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Daemon.Repository.EF.Enums;
    using Daemon.Common.Helpers;
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;

    public class TableInfoRepository : Repository<TableInfo>, ITableInfoRepository
    {
        protected override Expression<Func<TableInfo, int>> PrimaryKeyExpression => r => r.Id;

        private readonly IHttpContextAccessor _httpContextAccessor;


        public TableInfoRepository(ApiDBContent context, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override IQueryable<TableInfo> SelectEntitiesBeforeSelect(IQueryable<TableInfo> entities, ApiDBContent db)
        {
            entities = GetMyTableInfo(entities);
            entities = GetCommonTableInfo(entities);
            return entities;
        }

        private IQueryable<TableInfo> GetMyTableInfo(IQueryable<TableInfo> entities)
        {
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, TableInfoRelationshipEnum.MyTableInfo))
            {
                return entities;
            }

            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(r => r.Type == "UserId")?.Value;
            entities = entities.Where(r =>r.UserId == Convert.ToInt32(userId));
            return entities;
        }

        private IQueryable<TableInfo> GetCommonTableInfo(IQueryable<TableInfo> entities)
        {
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, TableInfoRelationshipEnum.Common))
            {
                return entities;
            }

            entities = entities.Where(r => r.IsPublic == 1);
            return entities;
        }
    }
}
