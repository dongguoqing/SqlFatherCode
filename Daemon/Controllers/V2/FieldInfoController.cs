namespace Daemon.Controllers
{
    using Daemon.Model;
    using Microsoft.AspNetCore.Mvc;
    using Daemon.Repository.Contract;
    using System.Net.Http;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;
    using Daemon.Common.Enums;
    using Daemon.Service.Contract;
    using Daemon.Common;
    [ApiController]
    [Route("v2/blueearth/field_info")]
    public class FieldInfoController : BaseApiController<FieldInfo, IFieldInfoRepository>
    {

        private readonly IFieldInfoService _fieldInfoService;
        public FieldInfoController(IFieldInfoRepository repository, IHttpContextAccessor httpContextAccessor, IFieldInfoService fieldInfoService) : base(repository, httpContextAccessor)
        {
            _fieldInfoService = fieldInfoService;
        }

        [HttpPost, Route("sql")]
        public ActionResult GenerateCreateSql([FromBody] int id)
        {
            return new ResultModel(System.Net.HttpStatusCode.OK, "", new List<string>() { _fieldInfoService.GenerateCreateSql(id) });
        }
    }
}
