using Daemon.Repository.Contract;
using Daemon.Service.Contract;
using Daemon.Common.Exceptions;
using Daemon.Common.Enums;
using Daemon.Common.SqlParser.Schema;
using Daemon.Common.SqlParser;
using Daemon.Model;
using System.Collections.Generic;
using System.Linq;

namespace Daemon.Service
{
    public class FieldInfoService : IFieldInfoService
    {
        private readonly IFieldInfoRepository _fieldInfoRepository;
        public FieldInfoService(IFieldInfoRepository fieldInfoRepository)
        {
            _fieldInfoRepository = fieldInfoRepository;
        }

        public string GenerateCreateSql(int id)
        {
            if (id <= 0)
            {
                throw new BusinessException((int)ErrorCode.PARAMS_ERROR, "");
            }

            var fieldInfo = _fieldInfoRepository.FindById(id);
            if (fieldInfo == null)
            {
                throw new BusinessException((int)ErrorCode.NOT_FOUND_ERROR, "");
            }

            Field field = Newtonsoft.Json.JsonConvert.DeserializeObject<Field>(fieldInfo.Content);
            SqlBuilder sqlBuilder = new SqlBuilder();
            return sqlBuilder.buildCreateFieldSql(field);
        }

        public FieldInfo FindById(int id)
        {
            return _fieldInfoRepository.FindById(id);
        }

        public IQueryable<FieldInfo> GetFieldInfos()
        {
            return _fieldInfoRepository.FindAll();
        }
    }
}