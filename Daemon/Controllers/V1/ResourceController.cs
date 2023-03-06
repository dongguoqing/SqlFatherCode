
using Daemon.Model;
using Microsoft.AspNetCore.Mvc;
using Daemon.Repository.Contract;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
namespace Daemon.Controllers.V1
{
    [ApiController]
    [Route("blueearth/resource")]
    public class ResourceController: BaseApiController<Resource, IResourceRepository>
    {
        public ResourceController(IResourceRepository repository,IHttpContextAccessor httpContextAccessor) : base(repository, httpContextAccessor)
        {
        }
    }
}
