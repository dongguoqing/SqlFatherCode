using System.Linq;
using Daemon.Common;
using Daemon.Infrustructure.EF;
using Daemon.Repository.Contract;
using Daemon.Model;
using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Daemon.Common.Exceptions;
using System.Collections.Generic;
using Daemon.Repository.EF.Enums;
using Daemon.Common.Helpers;

namespace Daemon.Repository.EF
{
    public class FileDirectoryRepository : Repository<FileDirectory>, IFileDirectoryRepository
    {
        protected override Expression<Func<FileDirectory, int>> PrimaryKeyExpression => r => r.Id;

        public FileDirectoryRepository(ApiDBContent context) : base(context)
        {
        }

        /// <summary>
        /// 重复的不能加
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="context"></param>
        /// <param name="isNew"></param>
        protected override void CheckDuplicateField(FileDirectory entity, ApiDBContent context, bool isNew = true)
        {
            base.CheckDuplicateField(entity, context, isNew);
            if (string.IsNullOrEmpty(entity.Name))
            {
                return;
            }

            var existDirectory = this.Context.FileDirectory.FirstOrDefault(r => r.Name == entity.Name && r.Superior == entity.Superior);
            if (existDirectory != null)
            {
                if (!isNew && existDirectory?.Id == entity.Id)
                {
                    return;
                }

                throw new ValueDuplicateException("名称为'" + entity.Name + "'已存在.");
            }
        }

        protected override IEnumerable<FileDirectory> IncludeRelationships(IEnumerable<FileDirectory> entities, ApiDBContent context)
        {
            if (RouteParameterHelper.CheckRouteParameter(RelationshipNames, FileDirectoryRelationshipEnum.ShowTopNode))
            {
                entities = BuildTreeNode(entities, 0);
            }
            return entities;
        }

        private IEnumerable<FileDirectory> BuildTreeNode(IEnumerable<FileDirectory> listAll, int pId)
        {
            List<FileDirectory> List = new List<FileDirectory>();

            if (pId != 0)
            {
                List = listAll.Where(A => A.Superior == pId).OrderBy(A => A.Name).ToList();
            }
            else
            {
                List = listAll.Where(A => A.Superior == 0).ToList();
            }
            if (List != null)
            {
                foreach (var item in List)
                {
                    item.Children = BuildTreeNode(listAll, item.Id).ToList();
                    if (item.Children?.Count == 0)
                    {
                        item.Children = null;
                    }
                }
            }
            return List.AsQueryable();
        }
    }
}
