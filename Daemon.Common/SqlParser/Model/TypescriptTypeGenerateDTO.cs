using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Common.SqlParser.Model
{
    public class TypescriptTypeGenerateDTO
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
        private List<TypeScriptFieldDTO> fieldList;

        public List<TypeScriptFieldDTO> FieldList { get => fieldList; set => fieldList = value; }
    }

    /**
     * 列信息
     */
    public class TypeScriptFieldDTO
    {

        /**
         * 字段名
         */
        private string fieldName;

        public string FieldName { get => fieldName; set => fieldName = value; }

        /**
         * Typescript 类型
         */
        private string typescriptType;

        public string TypescriptType { get => typescriptType; set => typescriptType = value; }

        /**
         * 字段注释
         */
        private string comment;

        public string Comment { get => comment; set => comment = value; }
    }

}