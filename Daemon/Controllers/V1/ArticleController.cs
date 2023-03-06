
using Daemon.Model;
using Microsoft.AspNetCore.Mvc;
using Daemon.Repository.Contract;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
namespace Daemon.Controllers.V1
{
    [ApiController]
    [Route("blueearth/article")]
    [Authorize(Policy = "Permission")]

    public class ArticleController : BaseApiController<Article, IArticleRepository>
    {
        public ArticleController(IArticleRepository repository,  IHttpContextAccessor httpContextAccessor) : base(repository, httpContextAccessor)
        {
        }
    }
}
