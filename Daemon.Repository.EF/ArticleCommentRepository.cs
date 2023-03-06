using System.Collections.Generic;
using Daemon.Infrustructure.EF;
using Daemon.Repository.Contract;
using Daemon.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using Daemon.Repository.EF.Enums;
using Daemon.Common.Helpers;
namespace Daemon.Repository.EF
{
	public class ArticleCommentRepository : Repository<ArticleComment>, IArticleCommentRepository
	{
		private readonly ApiDBContent _context;
		protected override Expression<Func<ArticleComment, int>> PrimaryKeyExpression => r => r.Id;

		public ArticleCommentRepository(ApiDBContent context) : base(context)
		{
			_context = context;
		}

		protected override IEnumerable<ArticleComment> IncludeRelationships(IEnumerable<ArticleComment> entities, ApiDBContent context)
		{

			//关联新增人以及回复人
			GetUserDic(entities);
			//关联评论详情（评论回复）
			SetApplyDetailOne(entities);
			return entities;
		}

		/// <summary>
		/// 获取当前评论的回复评论
		/// /// </summary>
		/// <returns></returns>
		private void SetApplyDetailOne(IEnumerable<ArticleComment> entities)
		{
			if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, ArticleCommentRelationshipEnum.CommentDetailOne))
			{
				return;
			}
			var commentIds = entities.Select(r => r.CommentId).Distinct();
			var commentsList = this._context.ArticleComment.Where(r => commentIds.Contains(r.CommentId)).ToList();

			foreach (var entity in entities)
			{
				if (entity.CommentId.HasValue)
					return;
				var commentDetail = commentsList.Where(r => r.CommentId == entity.Id && r.Id != entity.Id).ToList();
				entity.ApplyComment = commentDetail;
			}
		}

		/// <summary>
		/// 获取用户名
		/// </summary>
		/// <param name="entities"></param>
		/// <returns></returns>
		private void GetUserDic(IEnumerable<ArticleComment> entities)
		{
			var fromIds = entities.Select(r => r.AddUserId);
			var toIds = entities.Where(r => r.ToId.HasValue).Select(r => r.ToId.Value);
			var userDic = this._context.BlogUser.Where(r => fromIds.Concat(toIds).Contains(r.Id)).ToDictionary(r => r.Id, r => r.UserName);
		}

	}
}
