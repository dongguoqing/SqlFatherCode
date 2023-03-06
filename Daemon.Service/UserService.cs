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
using Daemon.Model.ViewModel;
using Daemon.Common.Exceptions;
using Daemon.Service.Contract.Jwt;
using Daemon.Common.Cache;
namespace Daemon.Service
{
    public class UserService : IUserService
    {
        private readonly IBlogSysInfoRepository _blogSysInfoRepository;

        private readonly IJwtAppService _jwtAppService;

        private readonly IUserRepository _userRepository;

        private readonly IDaemonDistributedCache _cache;

        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(IBlogSysInfoRepository blogSysInfoRepository, IUserRepository userRepository, IJwtAppService jwtAppService, IDaemonDistributedCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _blogSysInfoRepository = blogSysInfoRepository;
            _userRepository = userRepository;
            this._jwtAppService = jwtAppService;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        public ActionResult GetUserInfo()
        {
            var result = _httpContextAccessor.HttpContext.User.Claims.ToList();
            //get user id from claims
            var userId = result.FirstOrDefault(r => r.Type == "UserId")?.Value;
            //first get user from cache
            var userInfo = _cache.Get<User>(userId);
            if (userInfo == null)
            {
                userInfo = _userRepository.FindById(Convert.ToInt32(userId));
            }
            return new ResultModel(System.Net.HttpStatusCode.OK, "", new List<User> { userInfo });
        }

        public ActionResult Login(BaseUser user)
        {
            var sysInfo = _blogSysInfoRepository.FindAll().Where(a => a.InfoId == "transfinder" || a.InfoId == "sign").ToList();
            var curUser = _userRepository.FindAll().FirstOrDefault(a => a.UserAccount == user.UserAccount);
            if (curUser == null)
            {
                throw new NonexistentEntityException("");
            }
            _cache.Set<User>(curUser.Id.ToString(), curUser);
            if (curUser?.UserPassword != PasswordHelper.Rfc2898Encrypt(user?.PassWord, sysInfo.FirstOrDefault(a => a.InfoId == "transfinder")?.InfoValue))
            {
                throw new NonexistentEntityException("");
            }
            var sign = sysInfo.FirstOrDefault(a => a.InfoId == "sign")?.InfoValue;
            var extraHeaders = new Dictionary<string, object>
                {
                    { "userName", user.UserName },
                };
            double exp = (DateTime.UtcNow.AddMinutes(30) - new DateTime(1970, 1, 1)).TotalSeconds;
            user.Avatar = curUser.UserAvatar;
            user.Id = curUser.Id;
            var token = _jwtAppService.Create(user);
            // var refreshToken = JwtHelper.generateRefreshToken(ipAddress());
            // refreshToken.UserId = dbUser.Id;
            user.Token = token.Token;
            // user.RefreshToken = refreshToken;
            // _refreshTokenRepository.Insert(user.RefreshToken);
            return new ResultModel(HttpStatusCode.OK, "登录成功！", new List<BaseUser> { user });
        }

        public ActionResult Register(User user)
        {
            user.IsDelete = 0;
            user.UserRole = "";
            user.CreateTime = DateTime.Now;
            user.UpdateTime = DateTime.Now;
            //if user exist or not
            var userExist = this._userRepository.FindAll().FirstOrDefault(r => r.UserName == user.UserName);
            if (userExist != null)
            {
                throw new DuplicateWaitObjectException();
            }
            //if user is not exist, encrypt password
            var sysInfo = _blogSysInfoRepository.FindAll().Where(a => a.InfoId == "transfinder").FirstOrDefault();
            user.UserPassword = PasswordHelper.Rfc2898Encrypt(user.UserPassword, sysInfo.InfoValue);
            var addUser = _userRepository.AddRangeWithRelationships(new List<User> { user }.AsEnumerable());
            var resultState = addUser.Count() > 0 ? State.Success : State.NotFound;
            return new ResultModel(HttpStatusCode.OK, ResultState.GetValue((int)resultState), addUser.Count());
        }
    }
}
