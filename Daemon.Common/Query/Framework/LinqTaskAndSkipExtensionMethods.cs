using System.Collections.Generic;
using System.Linq;
using System.Web;
using Daemon.Common.Middleware;

namespace Daemon.Common.Query.Framework
{
    public static class LinqTaskAndSkipExtensionMethods
    {
        private const string SKIP = "current";
        private const string TAKE = "pageSize";

        public static IQueryable<TDomainEntity> SkipTake<TDomainEntity>(this IQueryable<TDomainEntity> queryable, int? skip, int? take)
        {
            if (skip.HasValue)
            {
                queryable = queryable.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                queryable = take.Value != 0 ? queryable.Take(take.Value) : queryable.Where(c => 1 == 0);
            }

            return queryable;
        }

        public static IEnumerable<dynamic> SkipTake(this IEnumerable<dynamic> enumerable, int? skip, int? take)
        {
            if (skip.HasValue)
            {
                enumerable = enumerable.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                enumerable = take.Value != 0 ? enumerable.Take(take.Value) : enumerable.Where(c => 1 == 0);
            }

            return enumerable;
        }

        public static IQueryable<TDomainEntity> SkipTake<TDomainEntity>(this IQueryable<TDomainEntity> queryable)
        {
            string takeValue = HttpContextHelper.Current.Request.Query[TAKE], skipValue = HttpContextHelper.Current.Request.Query[SKIP];
            if (!string.IsNullOrEmpty(takeValue))
            {
                skipValue = string.IsNullOrEmpty(skipValue) ? "1" : skipValue;
                if (int.TryParse(skipValue, out int skip) && int.TryParse(takeValue, out int take))
                {
                    //这里根据框架改造一下 直接后台计算
                    queryable = queryable.Skip((skip - 1) * take).Take(take);
                }
            }

            return queryable;
        }

        public static bool HasSkipTakeFromQueryString()
        {
            bool hasPaged = false;
            string takeValue = HttpContextHelper.Current.Request.Query[TAKE], skipValue = HttpContextHelper.Current.Request.Query[SKIP];
            if (!string.IsNullOrEmpty(takeValue))
            {
                skipValue = string.IsNullOrEmpty(skipValue) ? "1" : skipValue;
                if (int.TryParse(skipValue, out _) && int.TryParse(takeValue, out _))
                {
                    hasPaged = true;
                }
            }

            return hasPaged;
        }

        public static int PageSize()
        {
            int pageSize = 0;
            string takeValue = HttpContextHelper.Current.Request.Query[TAKE];
            if (!string.IsNullOrEmpty(takeValue))
            {
                if (int.TryParse(takeValue, out int size))
                {
                    pageSize = size;    
                }
            }
            return pageSize;
        }

        public static int PageIndex()
        {
            int PageIndex = 0;
            string skipValue = HttpContextHelper.Current.Request.Query[SKIP];
            if (!string.IsNullOrEmpty(skipValue))
            {
                if (int.TryParse(skipValue, out int size))
                {
                    PageIndex = size;
                }
            }
            return PageIndex;
        }
    }
}
