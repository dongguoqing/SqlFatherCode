using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Daemon.Model
{
    public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
    {
        private static int m_Marker = 0;

        /// <summary>
        /// 改变模型映射，只要Create返回的值跟上次缓存的值不一样，EFCore就认为模型已经更新，需要重新加载
        /// </summary>
        public static void ChangeTableMapping()
        {
            Interlocked.Increment(ref m_Marker);
        }

        /// <summary>
        /// 重写方法
        /// </summary>
        /// <param name="context">context模型</param>
        /// <returns></returns>
        public object Create(DbContext context)
        {
            return (context.GetType(), m_Marker);
        }
    }
}
