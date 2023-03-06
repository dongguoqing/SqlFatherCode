using Daemon.Model;
using Microsoft.AspNetCore.Mvc;
using Daemon.Repository.Contract;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
namespace Daemon.Controllers.V1
{
    [ApiController]
    [Route("blueearth/bloguserrole")]
    public class BlogUserRoleController : BaseApiController<BlogUserRole, IBlogUserRoleRepository>
    {
        public BlogUserRoleController(IBlogUserRoleRepository repository, IHttpContextAccessor httpContextAccessor) : base(repository, httpContextAccessor)
        {
        }
    }
}
