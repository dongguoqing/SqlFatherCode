using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.SqlParser.Schema;
using Scriban;
using Daemon.Common.SqlParser.Model;
using Daemon.Common.Const;
using System.Text;
using Daemon.Common.Extensions;

namespace Daemon.Common.SqlParser.Builder
{
    public class FrontendCodeBuilder
    {
        public static string BuildTypeScriptTypeCode(TableSchema tableSchema)
        {
            // 传递参数
            TypescriptTypeGenerateDTO generateDTO = new TypescriptTypeGenerateDTO();
            string tableName = tableSchema.TableName;

            string tableComment = tableSchema.TableComment;
            string upperCamelTableName = tableName.Capitalize();
            // 类名为大写的表名
            generateDTO.ClassName = upperCamelTableName;//首字母需要转大写
            // 类注释为表注释 > 表名
            generateDTO.ClassComment = tableComment;
            if (string.IsNullOrEmpty(tableComment))
            {
                generateDTO.ClassComment = upperCamelTableName;
            }

            // 依次填充每一列
            List<TypeScriptFieldDTO> fieldDTOList = new List<TypeScriptFieldDTO>();
            foreach (var field in tableSchema.FieldList)
            {
                TypeScriptFieldDTO fieldDTO = new TypeScriptFieldDTO();
                fieldDTO.Comment = field.Comment;
                FieldType fieldType = FieldTypeConst.GetByValue(field.FieldType);
                if (fieldType == null)
                {
                    fieldType = FieldTypeConst.FieldTypeList.FirstOrDefault(r => r.Key == "TEXT");
                }
                fieldDTO.TypescriptType = fieldType.TypeScriptType;
                fieldDTO.FieldName = field.FieldName;
                fieldDTOList.Add(fieldDTO);
            }
            generateDTO.FieldList = fieldDTOList;
            StringBuilder sbStr = new StringBuilder();
            sbStr.AppendLine(@$" interface {generateDTO.ClassName} {{");
            Parallel.ForEach(fieldDTOList, field =>
          {
              sbStr.AppendLine(string.Format("   {0}:{1};", field.FieldName, field.TypescriptType));
          });
            sbStr.Append("}");
            return sbStr.ToString();
        }
    }
}