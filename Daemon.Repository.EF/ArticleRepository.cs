using System.Collections.Generic;
using System.Linq;
using Daemon.Infrustructure.EF;
using Daemon.Repository.Contract;
using Daemon.Model;
using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Daemon.Common.Helpers;
using Daemon.Repository.EF.Enums;
namespace Daemon.Repository.EF
{
    public class ArticleRepository :Daemon.Infrustructure.EF.Repository<Article>, IArticleRepository
    {
        private readonly ApiDBContent _context;
        protected override Expression<Func<Article, int>> PrimaryKeyExpression => r => r.Id;
        private readonly BlogDictionaryRepository _blogDictionaryRepository;
        public ArticleRepository(ApiDBContent context, BlogDictionaryRepository blogDictionaryRepository) : base(context)
        {
            _context = context;
            _blogDictionaryRepository = blogDictionaryRepository;
        }

        protected override IEnumerable<Article> IncludeRelationships(IEnumerable<Article> entities, ApiDBContent context)
        {
            var entityList = entities.ToList();
            Dictionary<int, BlogUser> dicAddUser = new Dictionary<int, BlogUser>();
            Dictionary<int, BlogUser> dicUpdateUser = new Dictionary<int, BlogUser>();
            //关联用户信息
            if (RouteParameterHelper.CheckRouteParameter(RelationshipNames, ArticleRelationshipEnum.AddUser))
            {
                var addUserIds = entities.Select(r => r.AddUserId).ToList();
                dicAddUser = this._context.BlogUser.Where(r => addUserIds.Contains(r.Id)).ToDictionary(r => r.Id, r => r);//这样写 指定了当前外面关联的外键关联表
            }

            if (RouteParameterHelper.CheckRouteParameter(RelationshipNames, ArticleRelationshipEnum.UpdateUser))
            {
                var updateUserIds = entities.Select(r => r.UpdateUserId).ToList();
                dicUpdateUser = this._context.BlogUser.Where(r => updateUserIds.Contains(r.Id)).ToDictionary(r => r.Id, r => r);//这样写 指定了当前外面关联的外键关联表
            }

            SetArticleType(entities);

            foreach (var entity in entities)
            {
                if (dicAddUser.TryGetValue(entity.AddUserId, out BlogUser addUser))
                {
                    entity.AddUser = addUser;
                }

                if (dicAddUser.TryGetValue(entity.UpdateUserId, out BlogUser updateUser))
                {
                    entity.UpdateUser = updateUser;
                }
            }
            return entities;
        }

        /// <summary>
        /// 获取文章类型
        /// </summary>
        /// <param name="entities"></param>
        private void SetArticleType(IEnumerable<Article> entities)
        {
            if (RouteParameterHelper.CheckRouteParameter(RelationshipNames, ArticleRelationshipEnum.ArticleType))
            {
                var typeIds = entities.Select(r => r.LanguageTypeId).ToList();
                var blogDictionary = _blogDictionaryRepository.GetDbQueryAsNoTracking().Where(r => typeIds.Contains(r.Id)).ToDictionary(r => r.Id, r => r);
                foreach (var entity in entities)
                {
                    if (entity.LanguageTypeId.HasValue)
                    {
                        if (blogDictionary.TryGetValue(entity.LanguageTypeId.Value, out BlogDictionary dictionary))
                        {
                            entity.Dic = dictionary;
                        }
                    }
                }
            }
        }

        protected override void UpdateEntitiesBeforeCreate(IEnumerable<Article> entities, ApiDBContent context)
        {
            foreach (var entity in entities)
            {
                entity.IsDelete = 0;//删除
                entity.State = 0;//默认未审核
                entity.ReadCount = 0;//阅读数量为0
                entity.AdmireCount = 0;//点赞数量为0
                entity.IsOfficial = 0;//官方
                entity.IsGood = 0;//好文
                entity.IsRecommend = 0;//推荐
                entity.AddTime = DateTime.Now;
            }
        }
    }
}
