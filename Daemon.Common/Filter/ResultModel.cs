using System.Drawing;
using System.Linq;
using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Daemon.Common.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Daemon.Common.Query.Framework;
using Daemon.Common.Extend;
using Daemon.Infrustructure.Contract;
using System.Runtime.Serialization;
using Daemon.Model;
using Daemon.Model.ViewModel;
namespace Daemon.Common
{
    public class ResultModel : ActionResult
    {
        public int? _statusCode;
        public string msg { get; set; }
        public object obj { get; set; }
        public ResultModel()
        {
        }
        public ResultModel(HttpStatusCode? statusCode)
        {
            this._statusCode = (int)statusCode;
        }

        public ResultModel(HttpStatusCode statusCode, string msg = null, object obj = null)
        {
            this._statusCode = (int)statusCode;
            this.msg = msg;
            this.obj = obj;
        }

         public ResultModel(int statusCode, string msg = null, object obj = null)
        {
            this._statusCode = (int)statusCode;
            this.msg = msg;
            this.obj = obj;
        }
    }

    public class ResultModel<T>
    {
        private HttpStatusCode? _statusCode;
        private string _message = string.Empty;
        private List<T> _items = new List<T>();

        public ResultModel(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        public ResultModel(HttpStatusCode statusCode, string message)
        {
            _statusCode = statusCode;
            _message = message;
        }

        public ResultModel(IEnumerable<T> items)
        {
            SetItems(items);
        }

        public ResultModel(IEnumerable<T> items, bool needPaging)
        {
            SetItems(items, needPaging);
        }

        public ResultModel(HttpStatusCode statusCode, IEnumerable<T> items)
        {
            _statusCode = statusCode;
            SetItems(items);
        }

        public ResultModel(HttpStatusCode statusCode, IEnumerable<T> items, string message)
        {
            SetItems(items);
            _statusCode = statusCode;
            _message = message;
        }

        public ResultModel(T item)
        {
            if (item != null)
            {
                SetItems(new List<T>() { item });
            }
            else
            {
                NotFoundResource();
            }
        }

        public ResultModel(HttpStatusCode statusCode, T item)
        {
            if (item != null)
            {
                SetItems(new List<T>() { item });
                _statusCode = statusCode;
            }
            else
            {
                NotFoundResource();
            }
        }

        public ResultModel(HttpStatusCode statusCode, T item, string message)
        {
            if (item != null)
            {
                SetItems(new List<T>() { item });
                _statusCode = statusCode;
                _message = message;
            }
            else
            {
                NotFoundResource();
            }
        }

        public IEnumerable<T> Items
        {
            get
            {
                return _items;
            }
        }

        private int _filteredRecordCount = 0;

        public int? FilteredRecordCount => _filteredRecordCount;

        /// <summary>
        /// This method is temporary until change the grid skip and take into this middle ware, this method will be deleted.
        /// </summary>
        /// <param name="count"></param>
        public void SetFilteredRecordCount(int count)
        {
            _filteredRecordCount = count;
        }

        private int? _totalRecordCount;

        public int? TotalRecordCount { get => _totalRecordCount.HasValue ? _totalRecordCount.Value : _items.Count; set => _totalRecordCount = value; }

        public string GetMessage()
        {
            return _message;
        }

        public HttpStatusCode GetStatusCode()
        {
            return HttpStatusCodeHelper.GetStatusCode(_statusCode, HttpContextHelper.Current.Request.Method);
        }

        private void SetItems(IEnumerable<T> enumerable, bool needPaging = true)
        {
            if (enumerable == null)
            {
                NotFoundResource();
                return;
            }

            if (HttpContextHelper.Current != null && string.Equals(HttpContextHelper.Current.Request.Method, "GET", StringComparison.OrdinalIgnoreCase) && needPaging)
            {
                bool hasPage = LinqTaskAndSkipExtensionMethods.HasSkipTakeFromQueryString();
                _items = enumerable.AsQueryable().SkipTake().ToList();
                _filteredRecordCount = hasPage ? enumerable.Count() : _items.Count;
            }
            else
            {
                _items = enumerable.ToList();
                _filteredRecordCount = _items.Count();
            }
        }

        private void NotFoundResource()
        {
            _statusCode = HttpStatusCode.NotFound;
            _message = "Failed to find object.";
        }
    }

    public class ResultModel<TEntity, TRepository>
        where TEntity : class
        where TRepository : class, IRepository<TEntity>
    {
        private bool _enforeceQueryStringParameters = false;
        private HttpStatusCode _statusCode { get; set; }
        public string msg { get; set; }
        private IEnumerable<object> _items;

        [IgnoreDataMember]
        public bool HasSort { get; set; } = false;
        public HttpStatusCode StatusCode
        {
            get
            {
                return _statusCode;
            }
        }
        public IEnumerable<object> Items
        {
            get
            {
                return _items;
            }
        }

        private int? _filteredRecordCount = 0;

        [IgnoreDataMember]
        public int? FilteredRecordCount => _filteredRecordCount;

        private int? _totalRecordCount;

        public int? TotalRecordCount { get => _totalRecordCount.HasValue ? _totalRecordCount.Value : 0; set => _totalRecordCount = value; }

        private bool _isValueResponse = false;
        [IgnoreDataMember]
        private TRepository _repository;

        public ResultModel(HttpStatusCode statueCode)
        {
            this._statusCode = statueCode;
        }

        public ResultModel(TEntity item)
        {
            if (item != null)
            {
                SetItems(new List<TEntity>() { item });
            }
            else
            {
                NotFoundResource();
            }
        }

        public ResultModel(HttpStatusCode statusCode, IEnumerable<TEntity> enumerable)
        {
            SetItems(enumerable);
            _statusCode = statusCode;
        }

        public ResultModel(IEnumerable<TEntity> enumerable)
        {
            SetItems(enumerable);
        }

        public ResultModel(HttpStatusCode stateCode, string msg = null, IEnumerable<TEntity> obj = null)
        {
            this._statusCode = stateCode;
            this.msg = msg;
            SetItems(obj);
        }

        public ResultModel(HttpStatusCode stateCode, string msg = null, TEntity obj = null)
        {
            this._statusCode = stateCode;
            this.msg = msg;
            SetItems(new List<TEntity>() { obj }.AsEnumerable());
        }

        private void SetItems(IEnumerable<TEntity> enumerable)
        {
            if (enumerable == null)
            {
                NotFoundResource();
                return;
            }
            SetItems(enumerable.ToList().AsQueryable());
        }

        public void SetItems(IQueryable<TEntity> queryable)
        {
            if (queryable == null)
            {
                NotFoundResource();
                return;
            }
            _totalRecordCount = queryable.Count();
            _repository = ServiceLocator.Resolve<TRepository>();
            if (string.Equals(HttpContextHelper.Current.Request.Method, "GET", StringComparison.OrdinalIgnoreCase) || _enforeceQueryStringParameters)
            {
                if (LinqCountExtensionMethods.IsGetCount())
                {
                    _isValueResponse = true;
                    _totalRecordCount = 0;
                    _filteredRecordCount = 0;


                    _items = new List<object>() { queryable.Filter().ColumnQuery().Count() };
                }
                else if (LinqExistExtensionMethods.IsGetExist())
                {
                    _isValueResponse = true;
                    _totalRecordCount = 0;
                    _filteredRecordCount = 0;
                    _items = new List<object>() { queryable.Filter().ColumnQuery().Any() };
                }
                else
                {
                    IQueryable<TEntity> filterQueryable = queryable.Filter().ColumnQuery();
                    bool hasPage = LinqTaskAndSkipExtensionMethods.HasSkipTakeFromQueryString(), hasRelationships = _repository.HasRelationships();
                    if (hasPage && !HasSort)
                    {
                        string sortName = _repository.GetPrimaryKeys();
                        if (!string.IsNullOrEmpty(sortName))
                        {
                            List<SortItem> sortItems = new List<SortItem>()
                            {
                                new SortItem() { Name = _repository.GetPrimaryKeys(), Direction = SortDirection.Ascending },
                            };
                            filterQueryable = filterQueryable.Sort(sortItems);
                        }
                    }

                    var aliasFunc = _repository.GetFieldAliasFunc();
                    int aliasCount = _repository.GetAliasCount();
                    if (hasPage)
                    {
                        _items = _repository.IncludeRelationships(filterQueryable.Sort().SkipTake()).Select(aliasFunc, aliasCount, hasRelationships).ToList();
                        _filteredRecordCount = filterQueryable.Count();
                        TotalRecordCount = hasPage ? filterQueryable.Count() : _items.Count();
                        _items = new List<PageInfo>()
                        {
                            new PageInfo(){
                              Records = _items,
                              Size = LinqTaskAndSkipExtensionMethods.PageSize(),
                              Total = TotalRecordCount,
                              Current = LinqTaskAndSkipExtensionMethods.PageIndex(),
                              Pages = Convert.ToInt32(Math.Ceiling((decimal)TotalRecordCount/LinqTaskAndSkipExtensionMethods.PageSize()))
                            }
                        }.AsEnumerable();
                    }
                    else
                    {
                        _items = _repository.IncludeRelationships(filterQueryable.Sort()).Select(aliasFunc, aliasCount, hasRelationships).SkipTake().ToList();
                        _filteredRecordCount = hasPage ? filterQueryable.Count() : _items.Count();
                        TotalRecordCount = hasPage ? filterQueryable.Count() : _items.Count();
                    }
                }
            }
            else
            {
                if (LinqCountExtensionMethods.IsGetCount())
                {
                    _isValueResponse = true;
                    _totalRecordCount = 0;
                    _filteredRecordCount = 0;
                    _items = new List<object>() { queryable.Count() };
                }
                else if (LinqExistExtensionMethods.IsGetExist())
                {
                    _isValueResponse = true;
                    _totalRecordCount = 0;
                    _filteredRecordCount = 0;
                    _items = new List<object>() { queryable.Any() };
                }
                else
                {
                    _items = _repository.IncludeRelationships(queryable).ToList();
                    _filteredRecordCount = _items.Count();
                }
            }
        }

        private void NotFoundResource()
        {
            this._statusCode = HttpStatusCode.NotFound;
            this.msg = "Failed to find object.";
        }
    }

    internal static class HttpStatusCodeHelper
    {
        public static HttpStatusCode GetStatusCode(HttpStatusCode? statusCode, string httpMethod)
        {
            if (statusCode == null)
            {
                if (string.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase))
                {
                    return HttpStatusCode.Created;
                }
                else
                {
                    return HttpStatusCode.OK;
                }
            }

            return statusCode.Value;
        }
    }
}
