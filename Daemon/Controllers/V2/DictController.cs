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
    using Daemon.Common.Exceptions;
    using Daemon.Common.SqlParser.Schema;
    using Daemon.Common.SqlParser;
    using Daemon.Common.SqlParser.Model.Vo;
    [ApiController]
    [Route("v2/blueearth/dict")]
    public class DictController : BaseApiController<Dict, IDictRepository>
    {
        private readonly IDictService _dictService;

        public DictController(IDictRepository repository, IHttpContextAccessor httpContextAccessor, IDictService dictService) : base(repository, httpContextAccessor)
        {
            _dictService = dictService;
        }

        [HttpPost, Route("sql")]
        public ActionResult GenerateCreateSql([FromBody] int id)
        {
            if (id <= 0)
            {
                throw new BusinessException((int)ErrorCode.PARAMS_ERROR, "");
            }
            Dict dict = _dictService.FindById(id);
            if (dict == null)
            {
                throw new BusinessException((int)ErrorCode.NOT_FOUND_ERROR, "");
            }
            // 根据词库生成 Schema
            TableSchema tableSchema = new TableSchema();
            string name = dict.Name;
            tableSchema.TableName = "dict";
            tableSchema.TableComment = "name";
            List<Field> fieldList = new List<Field>();
            Field idField = new Field();
            idField.FieldName = "id";
            idField.FieldType = "bigint";
            idField.NotNull = true;
            idField.Comment = "id";
            idField.PrimaryKey = true;
            idField.AutoIncrement = true;
            Field dataField = new Field();
            dataField.FieldName = "data";
            dataField.FieldType = "text";
            dataField.Comment = "数据";
            dataField.MockType = MockTypeEnum.DICT.ToString();
            dataField.MockParams = id.ToString();
            fieldList.Add(idField);
            fieldList.Add(dataField);
            tableSchema.FieldList = fieldList;
            return new ResultModel(System.Net.HttpStatusCode.OK, "", new List<GenerateVO>() { GeneratorFacade.generateAll(tableSchema) });
        }
    }
}
