using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Daemon.Model;
using Daemon.Model.Entities.DBContextExtension;
using Daemon.Common.Middleware;
using Daemon.Infrustructure.EF.Framework;
namespace Daemon.Infrustructure.EF.SqlScripts
{
    public interface IBaseSqlScriptCollection<T>
        where T : class
    {
        string GetInsertByEntity(T entity);

        string GetInsertByEntity(IEnumerable<T> entities);

        string GetUpdateByEntity(T entity);

        string GetUpdateByEntity(IEnumerable<T> entities);

        string GetDeleteByEntity(T entity);

        string GetDeleteByEntity(IEnumerable<T> entities);

        string GetRecordIDByWhereClause(string whereClause);

        List<SqlParameter> GetSqlParameters();
    }

    public abstract class BaseSqlScriptCollection<TContext, T> : IBaseSqlScriptCollection<T>
        where TContext : ApiDBContent, new()
        where T : class
    {
        protected virtual string Columns => string.Format("({0})", string.Join(",", TableMapping.ColumnMappings.Where(r => r.ColumnName != PrimaryKeyName).Select(r => "[" + r.ColumnName + "]").ToList()));

        protected abstract string PrimaryKeyName { get; }

        protected virtual string Insert => "INSERT INTO {0} {1} \r\nSELECT * FROM ({2}) vt";

        private TableMapping _tableMapping;

        protected TableMapping TableMapping
        {
            get
            {
                if (_tableMapping == null)
                {
                    TContext dbcontext = new TContext();
                    var dbContext = ServiceLocator.Resolve<ApiDBContent>();
                    _tableMapping = dbcontext.GetMapping<T>();
                }

                return _tableMapping;
            }
        }

        public virtual string GetInsertByEntity(T entity)
        {
            string select = GetSelectSqlScript(entity);
            return string.Format(Insert, TableMapping.TableName, Columns, select);
        }

        public virtual string GetInsertByEntity(IEnumerable<T> entities)
        {
            return string.Empty;
        }

        public virtual string GetUpdateByEntity(T entity)
        {
            return string.Empty;
        }

        public virtual string GetUpdateByEntity(IEnumerable<T> entities)
        {
            return string.Empty;
        }

        public virtual string GetRecordIDByWhereClause(string whereClause)
        {
            return string.Empty;
        }

        public virtual string GetDeleteByEntity(T entity)
        {
            return string.Empty;
        }

        public virtual string GetDeleteByEntity(IEnumerable<T> entities)
        {
            return string.Empty;
        }

        public virtual List<SqlParameter> GetSqlParameters()
        {
            return new List<SqlParameter>();
        }

        protected virtual string GetSelectSqlScript(T entity)
        {
            string columns = string.Join(",", TableMapping.ColumnMappings.Where(r => r.ColumnName != PrimaryKeyName)
                .Select(r => SqlScriptHelper.GenericsToString(r.PropertyInfo.GetValue(entity)) + " AS [" + r.ColumnName + "]").ToList());
            return string.Format("SELECT {0}", columns);
        }

        protected virtual string[] GetUnmodifiedFieldNames()
        {
            return new string[] { PrimaryKeyName, "CreatedOn", "CreatedBy", "CreateDateTime" };
        }
    }
}
