using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.SqlParser.Model;
using Daemon.Common.SqlParser.Schema;
using Daemon.Common.SqlParser.Model.Vo;
using Daemon.Common.Exceptions;
using Daemon.Common.SqlParser.Builder;
namespace Daemon.Common.SqlParser
{
    public class GeneratorFacade
    {
        public static GenerateVO generateAll(TableSchema tableSchema)
        {
            // 校验
            validSchema(tableSchema);
            SqlBuilder sqlBuilder = new SqlBuilder();

            // 构造建表 SQL
            string createSql = sqlBuilder.buildCreateTableSql(tableSchema);

            int? mockNum = tableSchema.MockNum;
            //生成模拟数据
            Dictionary<string, List<string>> dataList = DataBuilder.generateData(tableSchema, mockNum.Value);
            //生成插入sql
            string insertSql = sqlBuilder.buildInsertSql(tableSchema, dataList);
            // 生成数据 json
            string dataJson = JsonBuilder.GetDataListJson(dataList);
            //生成C#实体
            string csharpEntityCode = CSharpCodeBuilder.BuildCsharpEntityCode(tableSchema);

            // 生成 typescript 类型代码
            String typescriptTypeCode = FrontendCodeBuilder.BuildTypeScriptTypeCode(tableSchema);
            // 封装返回
            GenerateVO generateVO = new GenerateVO();
            generateVO.TableSchema = tableSchema;
            generateVO.CreateSql = createSql;
            generateVO.DataList = dataJson;
            generateVO.insertSql = insertSql;
            generateVO.DataJson = dataJson;
            generateVO.CSharpEntityCode = csharpEntityCode;
            generateVO.TypescriptTypeCode = typescriptTypeCode;
            return generateVO;
        }

        /**
 * 验证 schema
 *
 * @param tableSchema 表概要
 */
        public static void validSchema(TableSchema tableSchema)
        {
            if (tableSchema == null)
            {
                throw new BusinessException("数据为空");
            }
            string tableName = tableSchema.TableName;
            if (string.IsNullOrEmpty(tableName))
            {
                throw new BusinessException("表名不能为空");
            }
            int? mockNum = tableSchema.MockNum;
            // 默认生成 20 条
            if (!mockNum.HasValue)
            {
                tableSchema.MockNum = 20;
                mockNum = 20;
            }
            if (mockNum > 100 || mockNum < 10)
            {
                throw new BusinessException("生成条数设置错误");
            }
            List<Field> fieldList = tableSchema.FieldList;
            if (!fieldList.Any())
            {
                throw new BusinessException("字段列表不能为空");
            }
            foreach (var field in fieldList)
            {
                validField(field);
            }
        }

        /**
   * 校验字段
   *
   * @param field
   */
        public static void validField(Field field)
        {
            string fieldName = field.FieldName;
            string fieldType = field.FieldType;
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new BusinessException("字段名不能为空");
            }
            if (string.IsNullOrEmpty(fieldType))
            {
                throw new BusinessException("字段类型不能为空");
            }
        }
    }
}