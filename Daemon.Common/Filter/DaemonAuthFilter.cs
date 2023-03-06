using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
// using System.Web.Http.Filters;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Daemon.ViewModel;
using Daemon.Common.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Daemon.Service.Contract;

namespace Daemon.Common.Filter
{
    /// <summary>
    /// 权限过滤中间件
    /// </summary>
    public class DaemonAuthFilter : ActionFilterAttribute
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly IBlogSysInfoService _blogSysInfoService;
        public DaemonAuthFilter(IConfiguration configuration,  IMemoryCache memoryCache, IBlogSysInfoService blogSysInfoService)
        {
            this._configuration = configuration;
            _memoryCache = memoryCache;
            _blogSysInfoService = blogSysInfoService;
        }

        /// <summary>
        /// 接口调用之前执行此方法
        /// </summary>
        /// <param name="context"></param>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var _context = context.HttpContext;
            var headerToken = _context.Request.Headers["token"];
            if (string.IsNullOrEmpty(headerToken))
                headerToken = _context.Request.Query["token"];
            var token = headerToken.ToString().Replace("Bearer ", "");
            Console.WriteLine(_context.Request.Headers.ContainsKey("token"));
            if (!_context.Request.Headers.ContainsKey("token"))
                _context.Request.Headers.Add("token", headerToken);
            var attributeList = (context.ActionDescriptor as ControllerActionDescriptor).MethodInfo.GetCustomAttributes(true);
            //如果添加了不校验注解 就不校验token了
            var authorityAttribute = (DescriptionNameAttribute)attributeList.Where(a => a.GetType().Name == typeof(DescriptionNameAttribute).Name).ToList().FirstOrDefault();
            var currentAuthId = authorityAttribute?.AuthorityId;
            //获取当前请求的接口路径
            var path = _context.Request.Path.Value;

            if (authorityAttribute?.AllowScope == 0)
            {
                await next();
                return;
            }
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new ResultModel(HttpStatusCode.Unauthorized, "请重新登录！", null);
                return;
            }
            var validateResult = "";
            switch (validateResult)
            {
                case "expired":
                case "error":
                    context.Result = new ResultModel(HttpStatusCode.Unauthorized, "请重新登录！", null);
                    return;
                case "invalid":
                    context.Result = new ResultModel(HttpStatusCode.Unauthorized, "请重新登录！", null);
                    return;
            }

            await next();
        }

        private string getSignCache()
        {
            if (!_memoryCache.TryGetValue("BlueearthSign", out string cacheValue))
            {
                cacheValue = _blogSysInfoService.GetByInfoId("sign")?.InfoValue;

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(3));

                _memoryCache.Set("BlueearthSign", cacheValue, cacheEntryOptions);
                return cacheValue;
            }
            else
            {
                return _memoryCache.Get("BlueearthSign")?.ToString();
            }

        }



        public class ValidateToken
        {
            public string refresh_token { get; set; }
            public string token_url { get; set; }
        }


        public class BE_PostPermissionList
        {
            public string permid { get; set; }

            public int ModuleId { get; set; }
        }



    }

}
