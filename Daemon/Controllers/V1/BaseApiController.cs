using System;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Collections.Generic;
using Daemon.Common;
using Daemon.Common.Filter;
using Daemon.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using Daemon.Infrustructure.Contract;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Daemon.Common.Exceptions;
using System.Linq;
using Daemon.Common.Extend;
using Daemon.Common.Query.Framework;
using System.ComponentModel.DataAnnotations;
using Daemon.Common.Attribute;
using System.Net;
using System.Threading;
namespace Daemon.Controllers
{
    public abstract class BaseApiController<TEntity, TRepository, PrimaryKeyType> : ControllerBase
    where TEntity : class, new()
    where TRepository : class, IRepository<TEntity, PrimaryKeyType>
    {
        protected TRepository _repository { get; private set; }
        protected IHttpContextAccessor _context { get; set; }
        protected BaseApiController(TRepository repository, IHttpContextAccessor context)
        {
            _repository = repository;
            _context = context;
        }

        /// <summary>
        /// 根据id获取基本信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public virtual ResultModel<TEntity, TRepository> GetById(PrimaryKeyType id)
        {
            EnterExit e = new EnterExit();
            if (Convert.ToInt32(id) > 1)
            {
                Thread nt1 = new Thread(new ThreadStart(e.NonCriticalSection));
                nt1.Start();
                Thread nt2 = new Thread(new ThreadStart(e.NonCriticalSection));
                nt2.Start();
            }
            else
            {
                Thread ct1 = new Thread(new ThreadStart(e.CriticalSection));
                ct1.Start();
                Thread ct2 = new Thread(new ThreadStart(e.CriticalSection));
                ct2.Start();
            }
            var result = _repository.FindById(id);
            var resultState = result != null ? State.Success : State.NotFound;
            return new ResultModel<TEntity, TRepository>(result);
        }

        public class EnterExit
        {
            public int result = 0;

            public void NonCriticalSection()
            {
                System.Console.WriteLine("Entered Thread " + Thread.CurrentThread.GetHashCode());
                for (var i = 0; i <= 5; i++)
                {
                    System.Console.WriteLine("Result = " + result++ + "ThreadID" + Thread.CurrentThread.GetHashCode());
                    Thread.Sleep(1000);
                }

                System.Console.WriteLine(" Exiting Thread " + Thread.CurrentThread.GetHashCode());
            }

            public void CriticalSection()
            {
                Monitor.Enter(this);
                NonCriticalSection();
                Monitor.Exit(this);
            }
        }


        // <summary>
        // 获取当前所有数据信息
        // </summary>
        // <returns></returns>
        [HttpGet, Route(nameof(GetAll))]
        public virtual ResultModel<TEntity, TRepository> GetAll()
        {
            return new ResultModel<TEntity, TRepository>(_repository.FindAll());
        }

        [Route("{id}")]
        [HttpPut]
        public virtual ResultModel<TEntity, TRepository> PutById([FromQuery] PrimaryKeyType id, [FromBody] TEntity entity)
        {
            if (_repository.FindById(id) == null)
            {
                throw new NonexistentEntityException();
            }

            string primaryKey = _repository.GetPrimaryKeys();
            var property = entity.GetType().GetProperty(primaryKey);
            if (property != null)
            {
                property.SetValue(entity, id);
            }

            return new ResultModel<TEntity, TRepository>(_repository.UpdateWithRelationships(entity));
        }

        [Route("")]
        [HttpPut]
        public virtual ResultModel<TEntity, TRepository> Put([FromBody] List<TEntity> entities)
        {
            return new ResultModel<TEntity, TRepository>(this._repository.UpdateRangeWithRelationships(entities));
        }

        /// <summary>
        /// create entity
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        public virtual ResultModel<TEntity, TRepository> Post([FromBody] List<TEntity> list)
        {
            return new ResultModel<TEntity, TRepository>(HttpStatusCode.Created, _repository.AddRangeWithRelationships(list));
        }

        /// <summary>
        /// remove data by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, HttpPost]
        [Route("")]
        public virtual ActionResult Delete([FromBody] List<PrimaryKeyType> ids)
        {
            int num = 0;
            IQueryable<TEntity> queryable;
            ids = ids ?? new List<PrimaryKeyType>();
            if (ids.Count != 0)
            {
                num = _repository.DeleteRangeByIds(ids);
            }
            else
            {
                queryable = _repository.FindAll();
                IQueryable<TEntity> queryableForFilter = queryable.Filter().ColumnQuery();
                if (queryable != queryableForFilter)
                {
                    num = _repository.DeleteRange(queryableForFilter.ToList());
                }
            }
            return new ResultModel(HttpStatusCode.OK, "", num);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        public virtual int DeleteById([FromQuery] PrimaryKeyType id)
        {
            TEntity entity = _repository.FindById(id);
            int num = _repository.Delete(entity);
            return num;
        }

        /// <summary>
        ///  Patch a single entity
        /// </summary>
        /// <param name="id">The entity id</param>
        /// <param name="patchData">The json data of which fields will be patched</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        public virtual ResultModel<TEntity, TRepository> PatchById([FromQuery] PrimaryKeyType id, [FromBody] List<JsonPatchData> patchData)
        {
            PatchDataTrim(patchData);
            return new ResultModel<TEntity, TRepository>(_repository.Patch(patchData, id));
        }

        /// <summary>
        /// Patch entities by ids
        /// </summary>
        /// <param name="ids">The entity ids</param>
        /// <param name="patchData">The json data of which fields will be patched</param>
        /// <returns>Return a successfully updated entities</returns>
        [Route("")]
        [HttpPatch]
        public virtual ResultModel<TEntity, TRepository> PatchByIds([Required] string ids, [FromBody] List<JsonPatchData> patchData)
        {
            PatchDataTrim(patchData);
            List<PrimaryKeyType> list = ids.Split(',').Select(r => (PrimaryKeyType)Convert.ChangeType(r, typeof(PrimaryKeyType))).ToList();
            return new ResultModel<TEntity, TRepository>(_repository.PatchRange(patchData, list));
        }

        /// <summary>
		/// Perform Massive Update against certain Fields for list of entities.
		/// </summary>
		/// <param name="massUpdateSettings">The settings of mass update</param>
		/// <param name="isMassUpdate">Must be true which means doing Massive Update.</param>
		/// <returns>Returns Update Result</returns>
		[Route("")]
        [HttpPatch]
        public virtual ResultModel<MassUpdateResult> PatchWithMassUpdateResult([FromBody] MassUpdateSettings massUpdateSettings, [Required][QueryKeyWord] bool isMassUpdate)
        {
            if (isMassUpdate)
            {
                return new ResultModel<MassUpdateResult>(_repository.MassUpdate(massUpdateSettings));
            }

            return new ResultModel<MassUpdateResult>(HttpStatusCode.NotFound);
        }

        protected void PatchDataTrim(List<JsonPatchData> patchData)
        {
            patchData.ForEach(r => r.Path = r.Path.StartsWith("/") ? r.Path.Substring(1) : r.Path);
        }
    }

    public abstract class BaseApiController<TEntity, TRepository> : BaseApiController<TEntity, TRepository, int>
     where TEntity : class, new()
     where TRepository : class, IRepository<TEntity, int>
    {
        protected BaseApiController(TRepository repository, IHttpContextAccessor context) : base(repository, context)
        {

        }
    }
}
