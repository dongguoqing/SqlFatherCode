
using Daemon.Repository.Contract;
using Daemon.Service.Contract;
using Daemon.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Daemon.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Daemon.Common.ViewModel;
using Daemon.Common.Filter;
using Daemon.Model.Entities;
using Daemon.Common.Const;
using Daemon.Common.Exceptions;
using Daemon.Service.Contract.Jwt;
using Daemon.Model.ViewModel;

namespace Daemon.Service
{
    public class BlogUserService : IBlogUserService
    {
        private readonly IBlogUserRepository _blogUserRepository;
        private readonly IBlogSysInfoRepository _blogSysInfoRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;
        private readonly IWebHostEnvironment _env;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtAppService _jwtAppService;
        private readonly AppSettings _appSettings;

        public BlogUserService(IBlogUserRepository blogUserRepository, IBlogSysInfoRepository blogSysInfoRepository,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env, IOptions<AppSettings> appSettings,
        IJwtAppService jwtAppService,
        IRefreshTokenRepository refreshTokenRepository)
        {
            this._blogUserRepository = blogUserRepository;
            this._blogSysInfoRepository = blogSysInfoRepository;
            this._configuration = configuration;
            this._context = httpContextAccessor;
            this._env = env;
            this._appSettings = appSettings.Value;
            this._refreshTokenRepository = refreshTokenRepository;
            this._jwtAppService = jwtAppService;
        }

        public ActionResult Login(BaseUser user)
        {
            var sysInfo = _blogSysInfoRepository.FindAll().Where(a => a.InfoId == "transfinder" || a.InfoId == "sign").ToList();
            var curUser = _blogUserRepository.FindAll().FirstOrDefault(a => a.UserName == user.UserName);
            if (curUser == null)
            {
                throw new NonexistentEntityException("");
            }
            if (curUser?.PassWord != PasswordHelper.Rfc2898Encrypt(user?.PassWord, sysInfo.FirstOrDefault(a => a.InfoId == "transfinder")?.InfoValue))
            {
                throw new NonexistentEntityException("");
            }
            var sign = sysInfo.FirstOrDefault(a => a.InfoId == "sign")?.InfoValue;
            var extraHeaders = new Dictionary<string, object>
                {
                    { "userName", user.UserName },
                };
            double exp = (DateTime.UtcNow.AddMinutes(30) - new DateTime(1970, 1, 1)).TotalSeconds;
            user.Avatar = curUser.Avatar;
            user.IdList = curUser.IdList;
            var token = _jwtAppService.Create(user);
            // var refreshToken = JwtHelper.generateRefreshToken(ipAddress());
            // refreshToken.UserId = dbUser.Id;
            user.Token = token.Token;
            // user.RefreshToken = refreshToken;
            user.Id = curUser.Id;
            // _refreshTokenRepository.Insert(user.RefreshToken);
            return new ResultModel(HttpStatusCode.OK, "登录成功！", user);
        }

        private AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            var refreshToken = this._refreshTokenRepository.FindAll().Where(r => r.Token == token).FirstOrDefault();
            var user = this._blogUserRepository.FindAll().FirstOrDefault(r => r.Id == refreshToken.Id);

            // return null if no user found with token
            if (user == null) return null;
            // return null if token is no longer active
            if (!refreshToken.IsActive) return null;

            // replace old refresh token with a new one and save
            var newRefreshToken = JwtHelper.generateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.Now;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            // _refreshTokenRepository.AddRange(refreshToken);
            // _refreshTokenRepository.Save();

            // generate new jwt
            var jwtToken = JwtHelper.CreateJwtToken(user, _appSettings.Secret);
            return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }

        private string ipAddress()
        {
            if (_context.HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
                return _context.HttpContext.Request.Headers["X-Forwarded-For"];
            else
                return _context.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }


        public ActionResult Register(BlogUser user)
        {
            user.IsEnabled = 1;
            //判断用户是否已经存在了
            var dbUser = _blogUserRepository.FindAll().FirstOrDefault(a => a.UserName == user.UserName);
            if (dbUser != null)
            {
                throw new DuplicateWaitObjectException();
            }
            var sysInfo = _blogSysInfoRepository.FindAll().Where(a => a.InfoId == "transfinder").FirstOrDefault();
            user.PassWord = PasswordHelper.Rfc2898Encrypt(user.PassWord, sysInfo.InfoValue);
            //校验一下手机号是否正确
            Match phone = Regex.Match(user.PhoneNumber, RegexConst.PhoneRegex);
            //校验一下邮箱是否正确
            Match email = Regex.Match(user.Email, RegexConst.EmailRegex);
            if (phone.Success && email.Success)
            {
                var addUser = _blogUserRepository.AddRangeWithRelationships(new List<BlogUser> { user }.AsEnumerable());
                var resultState = addUser.Count() > 0 ? State.Success : State.NotFound;
                return new ResultModel(HttpStatusCode.OK, ResultState.GetValue((int)resultState), addUser);
            }
            else
            {
                throw new NonexistentEntityException("手机号或者邮箱格式不正确！");
            }
        }

        public ActionResult ChangePassWord(BlogUser user)
        {
            var sign = _blogSysInfoRepository.FindAll().FirstOrDefault(a => a.InfoId == "sign")?.InfoValue;
            user.PassWord = PasswordHelper.Rfc2898Encrypt(user.PassWord, sign);
            this._blogUserRepository.UpdateWithRelationships(user);
            return new ResultModel(HttpStatusCode.OK, ResultState.GetValue((int)State.Success), null);
        }

        // public BlogUser CurrentUser()
        // {
        //     var headerToken = _context.HttpContext.Request.Headers["token"];
        //     if (string.IsNullOrEmpty(headerToken))
        //         headerToken = _context.HttpContext.Request.Query["token"];
        //     var token = headerToken.ToString().Replace("Bearer ", "");
        //     var signInfo = _blogSysInfoRepository.FindAll().FirstOrDefault(a => a.InfoId == "sign")?.InfoValue;
        //     try
        //     {
        //         var validateResult = JsonConvert.DeserializeObject<BlogUser>(JwtHelper.ValidateJwtToken(token, signInfo));
        //         var userInfo = _blogUserRepository.FindAll().FirstOrDefault(a => a.Id == validateResult.Id);
        //         return userInfo;
        //     }
        //     catch (Exception ex)
        //     {
        //         return null;
        //     }
        // }
    }
}
