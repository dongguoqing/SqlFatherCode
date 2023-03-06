using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Common.Tree
{
    public class  TreeNode<T>
      {
          public T Data { get; set; }
          public TreeNode<T> Parent { get; set; }
          public List<TreeNode<T>> Children { get; set; }
      }
}