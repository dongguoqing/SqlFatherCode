using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.SqlParser.Schema;

namespace Daemon.Common.SqlParser.Model.Vo
{
    public class GenerateVO
    {
        public TableSchema TableSchema;

        public String CreateSql;

        public string DataList;

        public string insertSql;

        public string DataJson;

        public string CSharpEntityCode;

        public string JavaObjectCode;

        public string TypescriptTypeCode;

        public static long SerialVersionUID = 7122637163626243606L;
    }
}