using Daemon.Common;
using Daemon.Service.Contract;
using Daemon.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Daemon.Repository.Contract;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Daemon.Common.CaptchaHelp;
using Daemon.Common.Filter;
namespace Daemon.Controllers.V1
{
    [ApiController]
    [Route("blueearth/dictionary")]
    public class BlogDictionaryController: BaseApiController<BlogDictionary, IBlogDictionaryRepository>
    {
        public BlogDictionaryController(IBlogDictionaryRepository repository,  IHttpContextAccessor httpContextAccessor) : base(repository, httpContextAccessor)
        {
        }     
    }
}
