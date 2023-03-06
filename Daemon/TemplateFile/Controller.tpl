namespace Daemon.Controllers.V2
{
    using Daemon.Model;
    using Microsoft.AspNetCore.Mvc;
    using Daemon.Repository.Contract;
    using System.Net.Http;
    using Daemon.Service.Contract;
    using Daemon.Common;
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    [ApiController]
    [Route("v2/blueearth/{1}")]
    public class {0}Controller : BaseApiController<{0}, I{0}Repository>
    {
        private readonly I{0}Service _{2}Service;
        public {0}Controller(I{0}Repository repository, IHttpContextAccessor httpContextAccessor,I{0}Service {2}Service) : base(repository, httpContextAccessor)
        {
            _{0}Service = {2}Service;
        }
    }
}
