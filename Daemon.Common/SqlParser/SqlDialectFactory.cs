using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Daemon.Common.Middleware;

namespace Daemon.Common.SqlParser
{
    public class SqlDialectFactory
    {
        private static Dictionary<string, ISqlDialect> defaultPool = new Dictionary<string, ISqlDialect>();

        private SqlDialectFactory()
        {

        }

        public static ISqlDialect GetDialect(string className)
        {
            ISqlDialect dialect = null;
            if (defaultPool.TryGetValue(className, out ISqlDialect sqlDialect))
            {
                dialect = sqlDialect;
            }
            if (null == dialect)
            {
                lock (className)
                {
                    dialect = ServiceLocator.Resolve<ISqlDialect>();
                    defaultPool.Add(className, dialect);
                }
            }
            return dialect;
        }
    }
}