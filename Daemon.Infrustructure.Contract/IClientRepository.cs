using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Daemon.Infrustructure.Contract
{
    public interface IClientRepository
    {
    }

    /// <summary>
    /// Repository标记接口
    /// </summary>
    public interface IClientRepository<TEntity> : IClientRepository,IDisposable
        where TEntity : class
    {

        IQueryable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");

        TEntity GetByID(object id);

        TEntity Insert(TEntity entity);
        List<TEntity> Insert(List<TEntity> entity);

        void Delete(object id);

        void Delete(TEntity entityToDelete);

        void Delete(Expression<Func<TEntity, bool>> predicate);

        bool Update(TEntity entityToUpdate);
        int ExcuteSql(string strSql, params SqlParameter[] parameters);
        bool Update(List<TEntity> entityToUpdate);

        void Insert<T>(List<T> data, string sqlconn, string TableName);

        void ChangeTable(string table);
        void ChangeDataBase(string connStr);

        void Save();
        int SaveChangesCount();
    }
}
