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
    [Route("v2/blueearth/table_info")]
    public class TableInfoController : BaseApiController<TableInfo, ITableInfoRepository>
    {
        private readonly ITableInfoService _tableInfoService;
        public TableInfoController(ITableInfoRepository repository, IHttpContextAccessor httpContextAccessor,ITableInfoService tableInfoService) : base(repository, httpContextAccessor)
        {
            _tableInfoService = tableInfoService;
        }

         [HttpPost, Route("sql")]
        public ActionResult GenerateCreateSql([FromBody] int id)
        {
            return new ResultModel(System.Net.HttpStatusCode.OK, "", new List<string>() { _tableInfoService.GenerateCreateSql(id) });
        }
    }
}
