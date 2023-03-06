using Daemon.Model;
using Microsoft.AspNetCore.Mvc;
using Daemon.Repository.Contract;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
namespace Daemon.Controllers.V1
{
    [ApiController]
    [Route("blueearth/directory")]
    public class FileDirectoryController : BaseApiController<FileDirectory, IFileDirectoryRepository>
    {
        public FileDirectoryController(IFileDirectoryRepository repository, IHttpContextAccessor httpContextAccessor) : base(repository, httpContextAccessor)
        {
        }
    }
}
