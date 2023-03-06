
using Daemon.Common;
using Daemon.Service.Contract;
using Daemon.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Daemon.Repository.Contract;
using Microsoft.AspNetCore.Http;
using Daemon.Common.CaptchaHelp;
using Daemon.Common.Filter;
namespace Daemon.Controllers.V1
{
    [ApiController]
    [Route("blueearth/blogroleresource")]
    public class BlogRoleResourceController : BaseApiController<BlogRoleResource, IBlogRoleResourceRepository>
    {
        private readonly IHostingEnvironment _env;
        private readonly ICaptcha _captCha;
        public BlogRoleResourceController(ICaptcha captCha, IBlogRoleResourceRepository repository, IHttpContextAccessor httpContextAccessor) : base(repository, httpContextAccessor)
        {
            this._captCha = captCha;
        }
    }
}
