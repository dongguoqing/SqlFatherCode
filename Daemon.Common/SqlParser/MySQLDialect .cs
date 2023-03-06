using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Common.SqlParser
{
    public class MySQLDialect : ISqlDialect
    {
        public string wrapFieldName(string name)
        {
            return string.Format("`{0}`", name);
        }

        /**
         * 解析字段名
         *
         * @param fieldName
         * @return
         */
        public string parseFieldName(string fieldName)
        {
            if (fieldName.StartsWith("`") && fieldName.EndsWith("`"))
            {
                return fieldName.Substring(1, fieldName.Length - 1);
            }
            return fieldName;
        }

        /**
         * 包装表名
         *
         * @param name
         * @return
         */
        public string wrapTableName(string name)
        {
            return string.Format("`{0}`", name);
        }

        /**
         * 解析表名
         *
         * @param tableName
         * @return
         */
        public string parseTableName(string tableName)
        {
            if (tableName.StartsWith("`") && tableName.EndsWith("`"))
            {
                return tableName.Substring(1, tableName.Length - 1);
            }
            return tableName;
        }
    }
}