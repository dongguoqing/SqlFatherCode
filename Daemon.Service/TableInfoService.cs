using Daemon.Repository.Contract;
using Daemon.Service.Contract;
using Daemon.Common.Exceptions;
using Daemon.Common.Enums;
using Daemon.Common.SqlParser.Schema;
using Daemon.Common.SqlParser;

namespace Daemon.Service
{
    public class TableInfoService: ITableInfoService
    {
        private readonly ITableInfoRepository _tableInfoRepository;
        public TableInfoService(ITableInfoRepository tableInfoRepository)
        {
            _tableInfoRepository = tableInfoRepository;
        }

        public string GenerateCreateSql(int id)
        {
            if (id <= 0)
            {
                throw new BusinessException((int)ErrorCode.PARAMS_ERROR, "");
            }

            var tableInfo = _tableInfoRepository.FindById(id);
            if (tableInfo == null)
            {
                throw new BusinessException((int)ErrorCode.NOT_FOUND_ERROR, "");
            }

            TableSchema field = Newtonsoft.Json.JsonConvert.DeserializeObject<TableSchema>(tableInfo.Content);
            SqlBuilder sqlBuilder = new SqlBuilder();
            return sqlBuilder.buildCreateTableSql(field);
        }
    }
}