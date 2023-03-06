using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.SqlParser;
using System.Reflection;
using Daemon.Common.SqlParser.Schema;
using System.Text;
using Daemon.Common.Exceptions;
using Daemon.Common.Enums;
using Daemon.Common.Const;

namespace Daemon.Common.SqlParser
{
    public class SqlBuilder
    {
        private ISqlDialect sqlDialect;

        public SqlBuilder()
        {
            this.sqlDialect = SqlDialectFactory.GetDialect(typeof(MySQLDialect).Name);
        }

        public SqlBuilder(ISqlDialect sqlDialect)
        {
            this.sqlDialect = sqlDialect;
        }

        /**
         * 构造建表 SQL
         *
         * @param tableSchema 表概要
         * @return 生成的 SQL
         */
        public string buildCreateTableSql(TableSchema tableSchema)
        {
            // 构造模板
            string template = "{0}\n"
                    + "create table if not exists {1}\n"
                    + "(\n"
                    + "{2}\n"
                    + ") {3};";
            // 构造表名
            string tableName = sqlDialect.wrapTableName(tableSchema.TableName);
            string dbName = tableSchema.DbName;
            if (!string.IsNullOrEmpty(dbName))
            {
                tableName = string.Format("{0}.{1}", dbName, tableName);
            }
            // 构造表前缀注释
            string tableComment = tableSchema.TableComment;
            if (string.IsNullOrEmpty(tableComment))
            {
                tableComment = tableName;
            }
            string tablePrefixComment = string.Format("-- {0}", tableComment);
            // 构造表后缀注释
            string tableSuffixComment = string.Format("comment '{0}'", tableComment);
            // 构造表字段
            List<Field> fieldList = tableSchema.FieldList;
            StringBuilder fieldStrBuilder = new StringBuilder();
            int fieldSize = fieldList.Count();
            for (int i = 0; i < fieldSize; i++)
            {
                Field field = fieldList[i];
                fieldStrBuilder.Append(buildCreateFieldSql(field));
                // 最后一个字段后没有逗号和换行
                if (i != fieldSize - 1)
                {
                    fieldStrBuilder.Append(",");
                    fieldStrBuilder.Append("\n");
                }
            }
            string fieldStr = fieldStrBuilder.ToString();
            // 填充模板
            string result = string.Format(template, tablePrefixComment, tableName, fieldStr, tableSuffixComment);
            return result;
        }

        /**
         * 生成创建字段的 SQL
         *
         * @param field
         * @return
         */
        public String buildCreateFieldSql(Field field)
        {
            if (field == null)
            {
                throw new BusinessException((int)ErrorCode.PARAMS_ERROR, "");
            }
            string fieldName = sqlDialect.wrapFieldName(field.FieldName);
            string fieldType = field.FieldType;
            string defaultValue = field.DefaultValue;
            bool notNull = field.NotNull;
            string comment = field.Comment;
            string onUpdate = field.OnUpdate;
            bool primaryKey = field.PrimaryKey;
            bool autoIncrement = field.AutoIncrement;
            // e.g. column_name int default 0 not null auto_increment comment '注释' primary key,
            StringBuilder fieldStrBuilder = new StringBuilder();
            // 字段名
            fieldStrBuilder.Append(fieldName);
            // 字段类型
            fieldStrBuilder.Append(" ").Append(fieldType);
            // 默认值
            if (!string.IsNullOrEmpty(defaultValue))
            {
                fieldStrBuilder.Append(" ").Append("default ").Append(getValueStr(field, defaultValue));
            }
            // 是否非空
            fieldStrBuilder.Append(" ").Append(notNull ? "not null" : "null");
            // 是否自增
            if (autoIncrement)
            {
                fieldStrBuilder.Append(" ").Append("auto_increment");
            }
            // 附加条件
            if (!string.IsNullOrEmpty(onUpdate))
            {
                fieldStrBuilder.Append(" ").Append("on update ").Append(onUpdate);
            }
            // 注释
            if (!string.IsNullOrEmpty(comment))
            {
                fieldStrBuilder.Append(" ").Append(string.Format("comment '{0}'", comment));
            }
            // 是否为主键
            if (primaryKey)
            {
                fieldStrBuilder.Append(" ").Append("primary key");
            }
            return fieldStrBuilder.ToString();
        }

        /**
         * 构造插入数据 SQL
         * e.g. INSERT INTO report (id, content) VALUES (1, '这个有点问题吧');
         *
         * @param tableSchema 表概要
         * @param dataList 数据列表
         * @return 生成的 SQL 列表字符串
         */
        public string buildInsertSql(TableSchema tableSchema, Dictionary<string, List<string>> dataList)
        {
            // 构造模板
            string template = "insert into {0} ({1}) values ({2});";
            // 构造表名
            string tableName = sqlDialect.wrapTableName(tableSchema.TableName);
            string dbName = tableSchema.DbName;
            if (!string.IsNullOrEmpty(dbName))
            {
                tableName = string.Format("{0}.{1}", dbName, tableName);
            }
            // 构造表字段
            List<Field> fieldList = tableSchema.FieldList;
            // 过滤掉不模拟的字段
            fieldList = fieldList.Where(r => r.MockType != MockTypeEnum.NONE.ToString()).ToList();
            StringBuilder resultStringBuilder = new StringBuilder();
            if (dataList.Values.Any())
            {
                int? valueLength = dataList.Values.ToList()[0]?.Count;
                for (int i = 0; i < valueLength; i++)
                {
                    string keyStr = string.Join(',', fieldList.Select(field => sqlDialect.wrapFieldName(field.FieldName)));
                    string valueStr = string.Join(',', fieldList.Select(field => getValueStr(field, dataList[field.FieldName]?[i])));
                    // 填充模板
                    string result = string.Format(template, tableName, keyStr, valueStr);
                    resultStringBuilder.Append(result);
                    // 最后一个字段后没有换行
                    if (i != valueLength - 1)
                    {
                        resultStringBuilder.Append("\n");
                    }
                }
            }
            return resultStringBuilder.ToString();
        }

        /**
         * 根据列的属性获取值字符串
         *
         * @param field
         * @param value
         * @return
         */
        public static String getValueStr(Field field, Object value)
        {
            if (field == null || value == null)
            {
                return "''";
            }
            FieldType fieldTypeEnum = FieldTypeConst.GetByValue(field.FieldType);
            if (fieldTypeEnum == null)
            {
                fieldTypeEnum = FieldTypeConst.FieldTypeList.FirstOrDefault(r => r.Key == "TEXT");
            }
            string result = value.ToString();
            switch (fieldTypeEnum.Key)
            {
                case "DATETIME":
                case "TIMESTAMP":
                    return result.Equals("CURRENT_TIMESTAMP") ? result : string.Format("'{0}'", value);
                case "DATE":
                case "TIME":
                case "CHAR":
                case "VARCHAR":
                case "TINYTEXT":
                case "TEXT":
                case "MEDIUMTEXT":
                case "LONGTEXT":
                case "TINYBLOB":
                case "BLOB":
                case "MEDIUMBLOB":
                case "LONGBLOB":
                case "BINARY":
                case "VARBINARY":
                    return string.Format("'{0}'", value);
                default:
                    return result;
            }
        }
    }
}