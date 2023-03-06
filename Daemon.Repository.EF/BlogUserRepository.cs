using System.ComponentModel;
using System.Collections.Generic;
using Daemon.Common;
using Daemon.Infrustructure.EF;
using Daemon.Repository.Contract;
using Daemon.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using Daemon.Repository.EF.Enums;
using Daemon.Common.Helpers;
using Daemon.Common.Extend;

namespace Daemon.Repository.EF
{
    public class BlogUserRepository : Repository<BlogUser>, IBlogUserRepository
    {
        protected override Expression<Func<BlogUser, int>> PrimaryKeyExpression => r => r.Id;
        private readonly IBlogUserRoleRepository _userRoleRepository;
        private readonly IResourceRepository _resourceRepository;
        public BlogUserRepository(ApiDBContent context, IBlogUserRoleRepository userRoleRepository, IResourceRepository resourceRepository) : base(context)
        {
            _userRoleRepository = userRoleRepository;
            _resourceRepository = resourceRepository;
        }

        /// <summary>
        /// 用户的密码需要做特殊处理 不能返回到前端 显示称***
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isNoTracking"></param>
        /// <returns></returns>
        public BlogUser GetByID(int id, bool isNoTracking = true)
        {
            var entity = FindById(id);
            var sign = this.Context.BlogSysInfo.FirstOrDefault(r => r.InfoId == "sign");
            SetPassWord(entity, sign?.InfoValue);
            return entity;
        }

        protected override IEnumerable<BlogUser> IncludeRelationships(IEnumerable<BlogUser> entities, ApiDBContent context)
        {
            //用户信息需要特殊处理 密码需要变成*不能显示出来
            var sign = GetSignInfo();
            Dictionary<int, string> dicUser = GetDicUser(entities);
            foreach (var entity in entities)
            {
                entity.PassWord = PasswordHelper.Rfc2898Decrypt(entity.PassWord, sign?.InfoValue);
                if (dicUser.TryGetValue(entity.AddUserId, out string addUserName))
                {
                    entity.AddUserName = addUserName;
                }

                if (dicUser.TryGetValue(entity.AddUserId, out string updateUserName))
                {
                    entity.UpdateUserName = updateUserName;
                }

                SetPassWord(entity, sign?.InfoValue);
            }

            GetResource(entities);

            return entities;
        }

        protected override IQueryable<BlogUser> SelectEntitiesBeforeSelect(IQueryable<BlogUser> entities, ApiDBContent context)
        {
            var userIds = entities.Select(r => r.Id).ToList();
            Dictionary<int, List<BlogRole>> dicRole = GetDicRole(entities);
            foreach (var entity in entities)
            {
                if (dicRole.TryGetValue(entity.Id, out List<BlogRole> roleList))
                {
                    entity.RoleName = String.Join(',', roleList.Select(r => r.Name));
                    entity.IdList = roleList.Select(r => r.Id).ToList();
                }
            }
            return entities;
        }

        /// <summary>
        /// 新增user之前执行
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="context"></param>
        protected override void UpdateEntitiesBeforeCreate(IEnumerable<BlogUser> entities, ApiDBContent context)
        {
            var sign = GetSignInfo();
            //需要对用户密码进行加密
            foreach (var entity in entities)
            {
                entity.IsEnabled = 1;
                EncryptPassWord(entity, sign?.InfoValue);
            }
        }

        /// <summary>
        ///新增user后执行
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="context"></param>
        protected override void UpdateEntitiesAfterCreate(IEnumerable<BlogUser> entities, ApiDBContent context)
        {
            var userRoleList = new List<BlogUserRole>();
            foreach (var entity in entities)
            {
                var idList = entity.IdList;
                idList.ForEach(r =>
                {
                    var userRole = new BlogUserRole();
                    userRole.RoleId = Convert.ToInt16(r);
                    userRole.UserId = entity.Id;
                    userRoleList.Add(userRole);
                });
            }
            _userRoleRepository.AddRangeWithRelationships(userRoleList);
        }

        protected override void SaveRelationships(BlogUser entity, BlogUser entityWithRelationships, ApiDBContent db, short entityORMType)
        {
            GetPassWord(entity);
        }

        /// <summary>
        /// 对密码进行加密
        /// </summary>
        /// <param name="user"></param>
        /// <param name="sign"></param>
        private void EncryptPassWord(BlogUser user, string sign)
        {
            user.PassWord = PasswordHelper.Rfc2898Encrypt(user.PassWord, sign);
        }

        private BlogSysInfo GetSignInfo()
        {
            var sign = this.Context.BlogSysInfo.FirstOrDefault(r => r.InfoId == "sign");
            return sign;
        }

        /// <summary>
        /// 获取用户的资源（菜单权限）
        /// </summary>
        private void GetResource(IEnumerable<BlogUser> entities)
        {
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, BlogUserRelationshipEnum.Resource))
            {
                return;
            }
            var roleIds = entities.Where(r => r.IdList != null).SelectMany(r => r.IdList).ToList();
            var allResources = _resourceRepository.IncludeRelationships(this.Context.Resource.ToList().AsQueryable()).Where(r => r.Superior == 0).AsNoTracking().ToList();
            var dicResources = (from a in this.Context.BlogRoleResource
                                join b in _resourceRepository.IncludeRelationships(this.Context.Resource) on a.ResourceId equals b.Id
                                select new { a.RoleId, b }).AsEnumerable().GroupBy(r => r.RoleId).ToDictionary(r => r.Key, r => r.Where(a => a.RoleId == r.Key).Select(a => a.b).ToList());
            foreach (var entity in entities)
            {

                //设置一个超级管理员 用于开发和管理
                if (entity.UserName == "admin")
                {
                    if (entity.ResourceList == null)
                    {
                        entity.ResourceList = new List<Resource>();
                    }
                    entity.ResourceList.AddRange(allResources);
                    continue;
                }

                if (entity.IdList == null)
                {
                    continue;
                }

                entity.IdList.ForEach(roleId =>
                {
                    if (dicResources.TryGetValue(roleId, out List<Resource> resources))
                    {
                        if (entity.ResourceList == null)
                        {
                            entity.ResourceList = new List<Resource>();
                        }
                        entity.ResourceList.AddRange(resources);
                    }
                });

                //去重
                entity.ResourceList = entity.ResourceList.DistinctBy(r => r.Id).ToList();
            }
        }

        /// <summary>
        /// 修改用户信息时，前端不会传password过来，所以需要重新获取一下password
        /// </summary>
        /// <param name="entity"></param>
        private void GetPassWord(BlogUser entity)
        {
            var userPassWord = this.Context.BlogUser.Where(r => r.Id == entity.Id).Select(r => r.PassWord).FirstOrDefault();
            entity.PassWord = userPassWord;
        }

        /// <summary>
        /// 获取用户的角色
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private Dictionary<int, List<BlogRole>> GetDicRole(IEnumerable<BlogUser> entities)
        {
            Dictionary<int, List<BlogRole>> dicRole = new Dictionary<int, List<BlogRole>>();
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, BlogUserRelationshipEnum.Role))
            {
                return dicRole;
            }
            var userIds = entities.Select(r => r.Id).ToList();
            var userRole = this.Context.BlogUserRole.Where(r => userIds.Contains(r.UserId)).ToList();
            dicRole = (from a in this.Context.BlogRole
                       join b in this.Context.BlogUserRole
                       on a.Id equals b.RoleId
                       where userIds.Contains(b.UserId)
                       select new { a, b.UserId }).AsEnumerable().GroupBy(r => r.UserId).ToDictionary(r => r.Key, r => r.Where(a => a.UserId == r.Key).Select(r => r.a).ToList());
            return dicRole;
        }

        /// <summary>
        /// 获取新增人名称
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        private Dictionary<int, string> GetDicUser(IEnumerable<BlogUser> entities)
        {
            Dictionary<int, string> dicUser = new Dictionary<int, string>();
            if (!RouteParameterHelper.CheckRouteParameter(RelationshipNames, BlogUserRelationshipEnum.AddUser))
            {
                return dicUser;
            }
            var addUserIds = entities.Select(r => r.AddUserId);
            var updateUserIds = entities.Select(r => r.UpdateUserId);
            dicUser = this.Context.BlogUser.AsEnumerable().Where(r => addUserIds.Concat(updateUserIds).Contains(r.Id)).ToDictionary(r => r.Id, r => r.UserName);
            return dicUser;
        }

        /// <summary>
        /// 将密码转换成*
        /// </summary>
        private void SetPassWord(BlogUser user, string sign)
        {
            string newPass = string.Empty;
            for (var i = 0; i < user.PassWord.Length; i++)
            {
                newPass += "*";
            }
            user.PassWord = newPass;
        }

    }
}
