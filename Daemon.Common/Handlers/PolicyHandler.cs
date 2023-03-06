namespace Daemon.Common.Handlers
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Daemon.Service.Contract.Jwt;
    using Microsoft.AspNetCore.Http;
    using System.Security.Claims;
    public class PolicyHandler : AuthorizationHandler<PolicyRequirement>
    {
        public IAuthenticationSchemeProvider Schemes { get; set; }


        private readonly IJwtAppService _jwtApp;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="schemes"></param>
        /// <param name="jwtApp"></param>
        public PolicyHandler(IAuthenticationSchemeProvider schemes, IJwtAppService jwtApp)
        {
            Schemes = schemes;
            _jwtApp = jwtApp;
        }

        //授权处理
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PolicyRequirement requirement)
        {
            //Todo：获取角色、Url 对应关系
            List<Menu> list = new List<Menu> {
                new Menu
                {
                    Role = Guid.Empty.ToString(),
                    Url = "/api/v1.0/Values"
                },
                new Menu
                {
                    Role=Guid.Empty.ToString(),
                    Url="/api/v1.0/secret/deactivate"
                },
                new Menu
                {
                    Role=Guid.Empty.ToString(),
                    Url="/api/v1.0/secret/refresh"
                }
            };

            var httpContext = (context.Resource as DefaultHttpContext).HttpContext;

            var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate != null)
            {
                var result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);
                if (result.Succeeded)
                {
                    if (!await _jwtApp.IsCurrentActiveTokenAsync())
                    {
                        context.Fail();
                        return;
                    }

                    httpContext.User = result.Principal;

                    var url = httpContext.Request.Path.Value.ToLower();
                    var role = httpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).FirstOrDefault().Value;
                    var isAdmin = Convert.ToBoolean(httpContext.User.Claims.Where(c => c.Type == "IsAdmin").FirstOrDefault().Value);
                    if (isAdmin)
                    {
                        context.Succeed(requirement);
                        return;
                    }
                    var menu = list.Where(x => x.Role.Equals(role) && x.Url.ToLower().Equals(url)).FirstOrDefault();

                    if (menu == null)
                    {
                        context.Fail();
                        return;
                    }
                    if (DateTime.Parse(httpContext.User.Claims.SingleOrDefault(s => s.Type == ClaimTypes.Expiration).Value) >= DateTime.UtcNow)
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail();
                    }
                    return;
                }
            }
            context.Fail();
        }

        public class Menu
        {
            public string Role { get; set; }

            public string Url { get; set; }
        }
    }
}
