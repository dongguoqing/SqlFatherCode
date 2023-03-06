namespace Daemon.Controllers.V2
{
    using Daemon.Model;
    using Microsoft.AspNetCore.Mvc;
    using Daemon.Repository.Contract;
    using System;
    using Microsoft.AspNetCore.Http;
    using Daemon.Common;
    using Daemon.Common.SqlParser.Builder;
    using Daemon.Common.SqlParser;
    using Daemon.Common.SqlParser.Schema;
    using System.Collections.Generic;
    using Daemon.Common.SqlParser.Model.Vo;
    using Daemon.Model.ViewModel;
    using Daemon.Common.Enums;
    using Daemon.Common.Exceptions;
    using System.IO;
    using Microsoft.EntityFrameworkCore;
    using Daemon.Common.Helpers;
    using Daemon.Common.Cache;
    using System.Linq;
    using System.Text;
    using Daemon.Common.Extensions;
    using System.Collections;

    [ApiController]
    [Route("v2/blueearth/sql")]
    public class SqlController : BaseApiController<Daemon.Model.TableInfo, ITableInfoRepository>
    {
        private readonly ApiDBContent _context;

        private readonly IDaemonDistributedCache _cache;

        private readonly IHttpContextAccessor _httpContextAccessor;
        public SqlController(ITableInfoRepository repository, IHttpContextAccessor httpContextAccessor, ApiDBContent context, IDaemonDistributedCache cache) : base(repository, httpContextAccessor)
        {
            _context = context;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }


        [HttpPost]
        [Route("sql")]
        public ResultModel SqlParse([FromBody] GenerateBySqlRequest sqlRequest)
        {
            if (sqlRequest == null)
            {
                throw new BusinessException((int)ErrorCode.PARAMS_ERROR, "");
            }
            // 获取 tableSchema
            return new ResultModel(System.Net.HttpStatusCode.OK, "", new List<TableSchema>() { TableSchemaBuilder.BuildFromSql(sqlRequest.sql) });
        }

        [Route("schema")]
        public ResultModel generateBySchema([FromBody] TableSchema tableSchema)
        {
            return new ResultModel(System.Net.HttpStatusCode.OK, "", new List<GenerateVO> { GeneratorFacade.generateAll(tableSchema) });
        }

        [Route("auto")]
        public ResultModel generateBySchema([FromBody] GenerateByAutoRequest autoRequest)
        {
            if (autoRequest == null)
            {
                throw new BusinessException((int)ErrorCode.PARAMS_ERROR, "");
            }
            return new ResultModel(System.Net.HttpStatusCode.OK, "", new List<TableSchema> { TableSchemaBuilder.buildFromAuto(autoRequest.Content) });
        }

        [Route("DownLoad")]
        [HttpPost]
        public ActionResult DownLoad([FromBody] GenerateVO schemal)
        {

            string filePath = @"D:\风景图片\爱心云\爱心云1.jpg";
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            return File(fileStream, "image/jpeg", "");//ms-excel
        }


        [Route("Test")]
        [HttpGet]
        public ActionResult Test()
        {

            string filePath = @"D:\风景图片\爱心云\爱心云1.jpg";
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            return File(fileStream, "image/jpeg", "");//ms-excel
        }

        [HttpPost, Route(nameof(DownloadFile))]
        public ActionResult DownloadFile([FromBody] RequestDownloadModel model)
        {
            var baseDirectory = new DirectoryInfo("..").FullName;
            switch (model.FileType)
            {
                case FileType.IRepository:
                    DirFile.CreateFile(baseDirectory + $"/Daemon.Repository.Contract/{model.FileName}", model.FileStr);
                    break;
                case FileType.Repositpry:
                    DirFile.CreateFile(baseDirectory + $"/Daemon.Repository.EF/{model.FileName}", model.FileStr);
                    break;
                case FileType.Service:
                    DirFile.CreateFile(baseDirectory + $"/Daemon.Service/{model.FileName}", model.FileStr);
                    break;
                case FileType.IService:
                    DirFile.CreateFile(baseDirectory + $"/Daemon.Service.Contract/{model.FileName}", model.FileStr);
                    break;
                case FileType.Controller:
                    DirFile.CreateFile(baseDirectory + $"/Daemon/Controllers/V2/{model.FileName}", model.FileStr);
                    break;
            }
            return new ResultModel(System.Net.HttpStatusCode.OK, "", new List<string> { model.FileStr });
        }

        [Route("GetSchemaList")]
        [HttpPost]
        public ActionResult GetSchemaList([FromBody] RequestConnectInfo connectInfo)
        {
            try
            {
                var connectionStr = $"Server={connectInfo.IpAddress};port={connectInfo.Port};Database=mysql;uid={connectInfo.UserName};pwd={connectInfo.PassWord};pooling=true;CharSet=utf8";
                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(r => r.Type == "UserId")?.Value;
                _cache.Set("ConnectInfo_" + userId, connectionStr);
                _context.Database.GetDbConnection().ConnectionString = connectionStr;
                List<SchemaInfo> result = _context.RawSqlQuery<SchemaInfo>("select DISTINCT TABLE_SCHEMA as TableName from information_schema.`TABLES` where  table_type = 'Base Table'", null, r => new SchemaInfo()
                {
                    TableName = r["TableName"] is DBNull ? null : r["TableName"].ToString(),
                });
                return new ResultModel(System.Net.HttpStatusCode.OK, "", result);
            }
            catch (Exception ex)
            {
                return new ResultModel((int)ErrorCode.PARAMS_ERROR, "数据库连接失败！", null);
            }
        }

        [Route("GetTableList")]
        [HttpGet]
        public ActionResult GetTableList([FromQuery] string databaseName)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(r => r.Type == "UserId")?.Value;
                _context.Database.GetDbConnection().ConnectionString = _cache.Get<string>("ConnectInfo_" + userId);
                List<SchemaInfo> result = _context.RawSqlQuery<SchemaInfo>($"SELECT TB.TABLE_NAME as TableName FROM  INFORMATION_SCHEMA.TABLES TB Where TB.TABLE_SCHEMA ='{databaseName}'", null, r => new SchemaInfo()
                {
                    TableName = r["TableName"] is DBNull ? null : r["TableName"].ToString(),
                    DatabaseName = databaseName
                });
                return new ResultModel(System.Net.HttpStatusCode.OK, "", result);
            }
            catch (Exception ex)
            {
                return new ResultModel((int)ErrorCode.OPERATION_ERROR, "数据库连接失败！", null);
            }
        }

        [Route("GetColumnList")]
        [HttpGet]
        public ActionResult GetColumnList([FromQuery] string tableName, string databaseName)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(r => r.Type == "UserId")?.Value;
                _context.Database.GetDbConnection().ConnectionString = _cache.Get<string>("ConnectInfo_" + userId);
                List<SchemaInfo> result = _context.RawSqlQuery<SchemaInfo>($"select COL.COLUMN_NAME as ColumnName from INFORMATION_SCHEMA.COLUMNS COL,INFORMATION_SCHEMA.TABLES TB where TB.table_name='{tableName}' AND TB.TABLE_NAME = COL.TABLE_NAME and TB.table_schema = '{databaseName}'", null, r => new SchemaInfo()
                {
                    TableName = r["ColumnName"] is DBNull ? null : r["ColumnName"].ToString(),
                });
                return new ResultModel(System.Net.HttpStatusCode.OK, "", result);
            }
            catch (Exception ex)
            {
                return new ResultModel((int)ErrorCode.OPERATION_ERROR, "数据库连接失败！", null);
            }
        }

        [Route("generate")]
        [HttpGet]
        public ActionResult GenerateCSharpCode(string tableName)
        {
            var result = new List<GenerateResult>();
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(r => r.Type == "UserId")?.Value;
            _context.Database.GetDbConnection().ConnectionString = _cache.Get<string>("ConnectInfo_" + userId);
            List<Daemon.Common.SqlParser.Schema.Field> fieldList = _context.RawSqlQuery<Daemon.Common.SqlParser.Schema.Field>($"select COL.COLUMN_NAME as ColumnName,COL.COLUMN_TYPE as FieldType from INFORMATION_SCHEMA.COLUMNS COL,INFORMATION_SCHEMA.TABLES TB where TB.table_name='{tableName}' AND TB.TABLE_NAME = COL.TABLE_NAME", null, r => new Daemon.Common.SqlParser.Schema.Field()
            {
                FieldName = r["ColumnName"] is DBNull ? null : r["ColumnName"].ToString().Capitalize(),
                FieldType = r["FieldType"] is DBNull ? null : r["FieldType"].ToString(),
            });
            var tableSchema = new TableSchema() { TableName = tableName, FieldList = fieldList };
            var repositoryTpl = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/TemplateFile/Repository.tpl").Replace("{0}", tableName.Capitalize());
            var iRepositoryTpl = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/TemplateFile/IRepository.tpl").Replace("{0}", tableName.Capitalize());
            var iServiceTpl = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/TemplateFile/IService.tpl").Replace("{0}", tableName.Capitalize());
            var serviceTpl = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/TemplateFile/Service.tpl").Replace("{0}", tableName.Capitalize());
            var controllerTpl = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/TemplateFile/Controller.tpl").Replace("{0}", tableName.Capitalize()).Replace("{1}", tableName).Replace("{2}", tableName);
            var generateResult = new GenerateResult() { ModelEntity = CSharpCodeBuilder.BuildCsharpEntityCode(tableSchema), ControllerStr = controllerTpl, RepositoryStr = repositoryTpl, IRepositoryStr = iRepositoryTpl, ServiceStr = serviceTpl, IServiceStr = iServiceTpl, RepositoryName = $"{tableName.Capitalize()}Repository.cs", IRepositoryName = $"I{tableName.Capitalize()}Repository.cs", ServiceName = $"{tableName.Capitalize()}Service.cs", IServiceName = $"I{tableName.Capitalize()}Service.cs" };
            result.Add(generateResult);

            return new ResultModel(System.Net.HttpStatusCode.OK, "", result);
        }
    }
}
