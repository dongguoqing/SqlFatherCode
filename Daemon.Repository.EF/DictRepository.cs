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
    using Newtonsoft.Json;

    public class DictRepository : Repository<Dict>, IDictRepository
    {
        protected override Expression<Func<Dict, int>> PrimaryKeyExpression => r => r.Id;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public DictRepository(ApiDBContent context, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void UpdateEntitiesBeforeCreate(IEnumerable<Dict> entities, ApiDBContent context)
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(r => r.Type == "UserId")?.Value;
            foreach (var entity in entities)
            {
                var content = entity.Content.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                entity.Content = JsonConvert.SerializeObject(content);
                entity.CreateTime = DateTime.Now;
                entity.UpdateTime = DateTime.Now;
                entity.UserId = Convert.ToInt32(userId);
            }
        }

        protected override IQueryable<Dict> SelectEntitiesBeforeSelect(IQueryable<Dict> entities, ApiDBContent context)
        {
            entities = GetMyDictInfo(entities);
            entities = GetCommonDictInfo(entities);
            return entities;
        }


        private IQueryable<Dict> GetMyDictInfo(IQueryable<Dict> entities)
        {
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, DictRelationshipEnum.MyFieldInfo))
            {
                return entities;
            }

            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(r => r.Type == "UserId")?.Value;
            entities = entities.Where(r => r.UserId == Convert.ToInt32(userId));
            return entities;
        }

        private IQueryable<Dict> GetCommonDictInfo(IQueryable<Dict> entities)
        {
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, DictRelationshipEnum.Common))
            {
                return entities;
            }

            entities = entities.Where(r => r.IsPublic == 1);
            return entities;
        }
    }
}
