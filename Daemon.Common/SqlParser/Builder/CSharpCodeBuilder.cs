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

namespace Daemon.Common.SqlParser
{
    public class CSharpCodeBuilder
    {
        public static string BuildCsharpEntityCode(TableSchema tableSchema)
        {
            CsharpEntityGenerateDto csharpEntityGenerateDto = new CsharpEntityGenerateDto();
            csharpEntityGenerateDto.ClassName = tableSchema.TableName.Capitalize();
            csharpEntityGenerateDto.ClassComment = tableSchema.TableComment;
            if (string.IsNullOrEmpty(csharpEntityGenerateDto.ClassComment))
            {
                csharpEntityGenerateDto.ClassComment = tableSchema.TableName;
            }
            // 依次填充每一列
            List<FieldDTO> fieldDTOList = new List<FieldDTO>();
            foreach (var field in tableSchema.FieldList)
            {
                FieldDTO fieldDTO = new FieldDTO();
                fieldDTO.Comment = field.Comment;
                FieldType fieldType = FieldTypeConst.GetByValue(field.FieldType);
                if (fieldType == null)
                {
                    fieldType = FieldTypeConst.FieldTypeList.FirstOrDefault(r => r.Key == "TEXT");
                }
                fieldDTO.CsharpType = fieldType.CSharpType;
                fieldDTO.FieldName = field.FieldName;
                fieldDTOList.Add(fieldDTO);
            }
            csharpEntityGenerateDto.FieldList = fieldDTOList;
            StringBuilder sbStr = new StringBuilder();
            sbStr.AppendLine(@$" public class {csharpEntityGenerateDto.ClassName} {{");
            Parallel.ForEach(fieldDTOList, field =>
            {
                sbStr.AppendLine(string.Format("  public {0} {1};", field.CsharpType, field.FieldName));
            });
            sbStr.AppendLine("}");
            return sbStr.ToString();
        }
    }
}