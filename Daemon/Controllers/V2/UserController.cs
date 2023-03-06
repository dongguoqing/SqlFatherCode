namespace Daemon.Controllers.V2
{
    using Daemon.Model;
    using Daemon.Repository.Contract;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Daemon.Service.Contract;
    using System.Collections.Generic;
    using System.Linq;
    [ApiController]
    [Route("v2/blueearth/user")]
    public class UserController : BaseApiController<User, IUserRepository>
    {
        private readonly IUserService _userService;
        public UserController(IUserRepository repository, IHttpContextAccessor httpContextAccessor, IUserService userService) : base(repository, httpContextAccessor)
        {
            _userService = userService;
        }

        [HttpPost, Route(nameof(Register))]
        public ActionResult Register([FromBody] User user)
        {
            return _userService.Register(user);
        }

        [HttpPost, Route(nameof(Login))]
        public ActionResult Login([FromBody] Daemon.Model.ViewModel.BaseUser user)
        {
            List<int> list = null;
            var a = list?.Select(a => a).ToList();
            return _userService.Login(user);
        }


        [HttpGet, Route("Get")]
        public ActionResult GetUserInfo()
        {
            return _userService.GetUserInfo();
        }
    }
}
