using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Common.SqlParser.Model
{
    public class CsharpEntityGenerateDto
    {
        /**
    * 类名
    */
        private string className;

        public string ClassName { get => className; set => className = value; }

        /**
         * 类注释
         */
        private string classComment;

        public string ClassComment { get => classComment; set => classComment = value; }

        /**
         * 列信息列表
         */
        private List<FieldDTO> fieldList;

        public List<FieldDTO> FieldList { get => fieldList; set => fieldList = value; }
    }

    public class FieldDTO
    {
        /**
         * 字段名
         */
        private string fieldName;

        public string FieldName { get => fieldName; set => fieldName = value; }

        /**
         * Java 类型
         */
        private string csharpType;

        public string CsharpType { get => csharpType; set => csharpType = value; }

        /**
         * 注释（字段中文名）
         */
        private string comment;

        public string Comment { get => comment; set => comment = value; }
    }

}