using System;
using System.Collections.Generic;
using System.Text;

namespace Daemon.Common.ViewModel
{
    public class AuthModel
    {
        /// <summary>
        /// 接口地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 请求方式
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool Disabled { get; set; }
        /// <summary>
        /// 验证方式（0匿名1登录2角色）
        /// </summary>
        public int AllowScope { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 权限id
        /// </summary>
        public string AuthorityId { get; set; }
        /// <summary>
        /// 权限id
        /// </summary>
        public string ModuleAuthRouteId { get; set; }
        public string ModuleName { get; set; }
    }
}
