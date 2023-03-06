using Daemon.Model;
using Microsoft.AspNetCore.Mvc;
using Daemon.Repository.Contract;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
namespace Daemon.Controllers.V1
{
    /// <summary>
    /// 文章评论
    /// </summary>
    [ApiController]
    [Route("blueearth/articlecomment")]
    public class ArticleCommentController : BaseApiController<ArticleComment, IArticleCommentRepository>
    {
        public ArticleCommentController(IArticleCommentRepository repository,  IHttpContextAccessor httpContextAccessor) : base(repository, httpContextAccessor)
        {
        }
    }
}
