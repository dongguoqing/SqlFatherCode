using Daemon.Infrustructure.EF;
using Daemon.Repository.Contract;
using System.Collections.Generic;
using Daemon.Model;
using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Daemon.Repository.EF.Enums;
using Daemon.Common.Helpers;
namespace Daemon.Repository.EF
{
	public class BlogDictionaryRepository : Repository<BlogDictionary>, IBlogDictionaryRepository
	{
		protected override Expression<Func<BlogDictionary, int>> PrimaryKeyExpression => r=>r.Id;
		private readonly ApiDBContent _context;
		public BlogDictionaryRepository(ApiDBContent context) : base(context)
		{
			this._context = context;
		}

		protected override IEnumerable<BlogDictionary> IncludeRelationships(IEnumerable<BlogDictionary> entities, ApiDBContent context)
		{
			Dictionary<int, BlogUser> dicAddUser = new Dictionary<int, BlogUser>();
			Dictionary<int, BlogUser> dicUpdateUser = new Dictionary<int, BlogUser>();
			if (RouteParameterHelper.CheckRouteParameter(RelationshipNames, BlogDictionaryRelationshipEnum.AddUser))
			{
				var addUserIds = entities.Select(r => r.AddUserId).ToList();
				dicAddUser = this._context.BlogUser.Where(r => addUserIds.Contains(r.Id)).ToDictionary(r => r.Id, r => r);//这样写 指定了当前外面关联的外键关联表
			}

			if (RouteParameterHelper.CheckRouteParameter(RelationshipNames, BlogDictionaryRelationshipEnum.UpdateUser))
			{
				var updateUserIds = entities.Select(r => r.UpdateUserId).ToList();
				dicUpdateUser = this._context.BlogUser.Where(r => updateUserIds.Contains(r.Id)).ToDictionary(r => r.Id, r => r);//这样写 指定了当前外面关联的外键关联表
			}

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

	}
}
