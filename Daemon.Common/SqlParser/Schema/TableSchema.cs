using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Common.SqlParser.Schema
{
    [Serializable]
    public class TableSchema
    {

        /**
         * 库名
         */
        private string dbName;

        /**
         * 表名
         */
        private string tableName;

        /**
         * 表注释
         */
        private string tableComment;

        /**
         * 模拟数据条数
         */
        private int? mockNum;

        public string DbName { get => dbName; set => dbName = value; }

        public string TableName { get => tableName; set => tableName = value; }

        public string TableComment { get => tableComment; set => tableComment = value; }

        public int? MockNum { get => mockNum; set => mockNum = value; }

        public List<Field> FieldList { get => fieldList; set => fieldList = value; }

        /**
         * 列信息列表
         */
        private List<Field> fieldList;
    }

    [Serializable]
    /**
        * 列信息
        */
    public class Field
    {
        /**
         * 字段名
         */
        private string fieldName;

        /**
         * 字段类型
         */
        private string fieldType;

        /**
         * 默认值
         */
        private string defaultValue;

        /**
         * 是否非空
         */
        private bool notNull;

        /**
         * 注释（字段中文名）
         */
        private string comment;

        /**
         * 是否为主键
         */
        private bool primaryKey;

        /**
         * 是否自增
         */
        private bool autoIncrement;

        /**
         * 模拟类型（随机、图片、规则、词库）
         */
        private string mockType;

        /**
         * 模拟参数
         */
        private string mockParams;

        /**
         * 附加条件
         */
        private string onUpdate;

        public string FieldName { get => fieldName; set => fieldName = value; }

        public string FieldType { get => fieldType; set => fieldType = value; }

        public string DefaultValue { get => defaultValue; set => defaultValue = value; }

        public bool NotNull { get => notNull; set => notNull = value; }

        public string Comment { get => comment; set => comment = value; }

        public bool PrimaryKey { get => primaryKey; set => primaryKey = value; }

        public bool AutoIncrement { get => autoIncrement; set => autoIncrement = value; }

        public string MockType { get => mockType; set => mockType = value; }

        public string MockParams { get => mockParams; set => mockParams = value; }

         public string OnUpdate { get => onUpdate; set => onUpdate = value; }

    }
}
