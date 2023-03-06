/*******************************************************************************
 * Copyright © 2016 .Framework 版权所有
 * Author: 
 * Description: 快速开发平台
 * Website：http://www..cn
 *********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Daemon.Common.Extend {
    public static class ExtList {
        /// <summary>
        /// 获取表里某页的数据
        /// </summary>
        /// <param name="data">表数据</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="allPage">返回总页数</param>
        /// <returns>返回当页表数据</returns>
        public static List<T> GetPage<T> (this List<T> data, int pageIndex, int pageSize, out int allPage) {
            allPage = 1;
            return null;
        }
        /// <summary>
        /// IList转成List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> IListToList<T> (IList list) {
            T[] array = new T[list.Count];
            list.CopyTo (array, 0);
            return new List<T> (array);
        }
        
        /// <summary>
        /// 去重
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> DistinctBy<T, TKey> (this IEnumerable<T> source, Func<T, TKey> keySelector) {
            HashSet<TKey> seenKeys = new HashSet<TKey> ();
            foreach (T element in source) {
                if (seenKeys.Add (keySelector (element))) {
                    yield return element;
                }
            }
        }
    }
}
