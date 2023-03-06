using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Common.Tree
{
     public enum  TeeNodeCompareResult
    {
        /// <summary>
        /// 树结点
        /// </summary>
        Parent,
        /// <summary>
        /// 子结点
        /// </summary>
        Child,
        /// <summary>
        /// 下一个同级结点
        /// </summary>
        NextNode,
        /// <summary>
        /// 前一个同级结点
        /// </summary>
        PreNode,
        /// <summary>
        /// 同一个结点
        /// </summary>
        EquealNode ,
        /// <summary>
        /// 下一层的结点
        /// </summary>
        NexLevelNode

    }
}