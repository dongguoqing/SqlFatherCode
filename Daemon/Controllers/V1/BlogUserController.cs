
using System.Security.AccessControl;
using System.Collections.Generic;
using Daemon.Common;
using Daemon.Service.Contract;
using Daemon.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;
using Daemon.Repository.Contract;
using Microsoft.AspNetCore.Http;
using Daemon.Common.CaptchaHelp;
using Daemon.Common.Filter;
using Microsoft.Extensions.Configuration;
using System.Net;
using Daemon.Common.Cache;
using Microsoft.AspNetCore.Authorization;
namespace Daemon.Controllers.V1
{
    [ApiController]
    [Route("blueearth/user")]
    [Authorize(Policy = "Permission")]
    public class BlogUserController : BaseApiController<BlogUser, IBlogUserRepository>
    {
        private readonly IBlogUserService _blogUserService;
        private readonly IHostingEnvironment _env;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICaptcha _captCha;

        private readonly IBlogSysInfoService _blogSysInfoService;

        private readonly IConfiguration _configuration;

        private readonly IDaemonDistributedCache _daemonDistributedCache;
        public BlogUserController(ICaptcha captCha, IBlogUserRepository repository, IBlogUserService blogUserService, IBlogSysInfoService blogSysInfoService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IDaemonDistributedCache daemonDistributedCache) : base(repository, httpContextAccessor)
        {
            this._blogUserService = blogUserService;
            this._captCha = captCha;
            this._httpContextAccessor = httpContextAccessor;
            _blogSysInfoService = blogSysInfoService;
            _configuration = configuration;
            _daemonDistributedCache = daemonDistributedCache;
        }

        [HttpPost, Route(nameof(Register))]
        public ActionResult Register([FromBody] BlogUser user)
        {
            return _blogUserService.Register(user);
        }

        [HttpGet, Route(nameof(GetCode))]
        [AllowAnonymous]
        public async Task<ActionResult> GetCode()
        {
            var code = await _captCha.GenerateRandomCaptchaAsync();
            var result = await _captCha.GenerateCaptchaImageAsync(code);
            var base64Str = Convert.ToBase64String(result.CaptchaMemoryStream.ToArray());
            result.Base64Str = base64Str;
            result.CaptchaMemoryStream = null;
            _daemonDistributedCache.Set("code", result.CaptchaCode);
            var cacheCode = _daemonDistributedCache.Get<string>("code");
            return new ResultModel(HttpStatusCode.OK, "获取验证码成功！", result);
        }

        [HttpPost, Route(nameof(Login))]
        [AllowAnonymous]
        [DescriptionName("1", "/blueearth/BlogUser/GetCode", "GET", true, 0, "登录")]
        public ActionResult Login([FromBody] Daemon.Model.ViewModel.BaseUser user)
        {
            List<int> list = null;
            var a = list?.Select(a => a).ToList();
            return _blogUserService.Login(user);
        }

        // [DescriptionName("1", "/blueearth/BlogUser/CurrentUser", "GET", true, 0, "获取用户信息")]
        // [HttpGet, Route(nameof(CurrentUser))]
        // public ActionResult CurrentUser()
        // {
        //     var userInfo = _blogUserService.CurrentUser();
        //     return new ResultModel(HttpStatusCode.OK, "获取用户信息成功！", userInfo); ;
        // }

        [HttpPut, Route(nameof(ChangePassWord))]
        public ActionResult ChangePassWord([FromBody] BlogUser user)
        {
            return _blogUserService.ChangePassWord(user);
        }

        // [HttpGet("revoke-token")]
        // public IActionResult RevokeToken(RevokeTokenRequest model)
        // {
        //     var token = model.Token ?? this._context.HttpContext.Request.Cookies["refreshToken"];

        //     if (string.IsNullOrEmpty(token))
        //         return BadRequest(new { message = "Token is required" });
        // }
    }
}
