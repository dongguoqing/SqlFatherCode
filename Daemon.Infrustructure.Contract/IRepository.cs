using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Daemon.Model.Entities;

namespace Daemon.Infrustructure.Contract
{
    public partial interface IRepository
    {
    }

    public partial interface IRepository<TEntity, TK> : IRepository<TEntity>, IDisposable
where TEntity : class, new()
    {
        MassUpdateResult MassUpdate(MassUpdateSettings massUpdateSettings);
    }

    public partial interface IRepository<TEntity> : IRepository
        where TEntity : class
    {
        void SetRelationships(List<Relationship> relationships);
        IQueryable<TEntity> IncludeRelationships(IQueryable<TEntity> entities, short? associatedDatabaseId = null);

        bool HasRelationships();

        int TotalCount();

        string GetPrimaryKeys();

        Func<string, string> GetFieldAliasFunc();

        int GetAliasCount();
    }

    public partial interface IRepository<TEntity, TK> : IRepository<TEntity>, IDisposable
    where TEntity : class, new()
    {
        IQueryable<TEntity> FindAll();

        TEntity FindById(TK id);

        IQueryable<TEntity> FindByIds(List<TK> ids);

        IEnumerable<TEntity> AddRangeWithRelationships(IEnumerable<TEntity> entities);

        TEntity UpdateWithRelationships(TEntity entity);

        IEnumerable<TEntity> UpdateRangeWithRelationships(IEnumerable<TEntity> entities);

        int Delete(TEntity entity);

        int DeleteRange(IQueryable<TEntity> entities);

        int DeleteRange(IEnumerable<TEntity> entities);

        int DeleteRangeByIds(IEnumerable<TK> ids);

        TEntity Patch(List<JsonPatchData> patchData, TK id);

        IEnumerable<TEntity> PatchRange(List<JsonPatchData> patchData);

        IEnumerable<TEntity> PatchRange(List<JsonPatchData> patchData, List<TK> ids);

    }

    public interface IBaseSingleTableRepository<T> : IRepository<T>
            where T : class
    {
        T Find();

        T Update(T entity);

        T Patch(List<JsonPatchData> patchData);
    }

}
