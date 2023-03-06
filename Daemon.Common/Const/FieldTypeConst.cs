using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Common.Const
{
    public class FieldTypeConst
    {
        public static List<FieldType> FieldTypeList = new List<FieldType>()
        {
           new FieldType(){ Key = "TINYINT",Value = "tinyint",CSharpType="int",TypeScriptType="number"  },
           new FieldType(){ Key = "SMALLINT",Value = "Integer",CSharpType="int",TypeScriptType="number"  },
           new FieldType(){ Key = "MEDIUMINT",Value = "mediumint",CSharpType="int",TypeScriptType="number"  },
           new FieldType(){ Key = "INT",Value = "int",CSharpType="int",TypeScriptType="number"  },
           new FieldType(){ Key = "BIGINT",Value = "bigint",CSharpType="long",TypeScriptType="number"  },
           new FieldType(){ Key = "FLOAT",Value = "float",CSharpType="double",TypeScriptType="number"  },
           new FieldType(){ Key = "DOUBLE",Value = "double",CSharpType="double",TypeScriptType="number"  },
           new FieldType(){ Key = "DECIMAL",Value = "decimal",CSharpType="decimal",TypeScriptType="number"  },
           new FieldType(){ Key = "DATE",Value = "date",CSharpType="DateTime",TypeScriptType="Date"  },
           new FieldType(){ Key = "TIME",Value = "time",CSharpType="Time",TypeScriptType="Date"  },
           new FieldType(){ Key = "YEAR",Value = "year",CSharpType="int",TypeScriptType="number"  },
           new FieldType(){ Key = "DATETIME",Value = "datetime",CSharpType="DateTime",TypeScriptType="Date"  },
           new FieldType(){ Key = "TIMESTAMP",Value = "timestamp",CSharpType="Long",TypeScriptType="number"  },
           new FieldType(){ Key = "CHAR",Value = "char",CSharpType="string",TypeScriptType="string"  },
           new FieldType(){ Key = "VARCHAR",Value = "varchar",CSharpType="string",TypeScriptType="string"  },
           new FieldType(){ Key = "TINYTEXT",Value = "tinytext",CSharpType="string",TypeScriptType="string"  },
           new FieldType(){ Key = "TEXT",Value = "text",CSharpType="string",TypeScriptType="string"  },
           new FieldType(){ Key = "MEDIUMTEXT",Value = "mediumtext",CSharpType="string",TypeScriptType="string"  },
           new FieldType(){ Key = "LONGTEXT",Value = "longtext",CSharpType="string",TypeScriptType="string"  },
           new FieldType(){ Key = "TINYBLOB",Value = "tinyblob",CSharpType="byte[]",TypeScriptType="string"  },
           new FieldType(){ Key = "BLOB",Value = "blob",CSharpType="byte[]",TypeScriptType="string"  },
           new FieldType(){ Key = "MEDIUMBLOB",Value = "mediumblob",CSharpType="byte[]",TypeScriptType="string"  },
           new FieldType(){ Key = "LONGBLOB",Value = "longblob",CSharpType="byte[]",TypeScriptType="string"  },
           new FieldType(){ Key = "BINARY",Value = "binary",CSharpType="byte[]",TypeScriptType="string"  },
           new FieldType(){ Key = "VARBINARY",Value = "varbinary",CSharpType="byte[]",TypeScriptType="string"  },

        };

        public static List<string> GetValues()
        {
            return FieldTypeList.Select(r => r.Value).ToList();
        }

        public static FieldType GetByValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return FieldTypeList.FirstOrDefault(r => r.Value == value);
        }
    }

    public class FieldType
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public string CSharpType { get; set; }

        public string TypeScriptType { get; set; }
    }
}