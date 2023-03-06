using Daemon.Common;
using Daemon.Service.Contract;
using Daemon.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Daemon.Repository.Contract;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Daemon.Common.CaptchaHelp;
using Daemon.Common.Filter;
namespace Daemon.Controllers.V1
{
    [ApiController]
    [Route("blueearth/role")]
    public class BlogRoleController : BaseApiController<BlogRole, IBlogRoleRepository>
    {
        public BlogRoleController(IBlogRoleRepository repository, IHttpContextAccessor httpContextAccessor) : base(repository, httpContextAccessor)
        {
        }
    }
}
