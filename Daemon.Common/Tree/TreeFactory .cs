using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Common.Tree
{
    public class TreeFactory<T>
    {
        public delegate TeeNodeCompareResult TeeNodeCompare<T>(T child, T parent);
        private TeeNodeCompare<T> compare;
        public TreeFactory(TeeNodeCompare<T> Compare) { this.compare = Compare; }

        public List<TreeNode<T>> CreateTreeByLevel
                    (List<T> Items)
        {
            Items.Sort(new Comparison<T>(this.CompareResult));
            List<TreeNode<T>> result = new List<TreeNode<T>>();
            TreeNode<T> lastNode = null;
            Queue<TreeNode<T>> queue = new Queue<TreeNode<T>>();
            TreeNode<T> currentNode = null;
            var current = result;
            if (Items.Count > 0)
            {



                for (int i = 0; i < Items.Count; i++)
                {



                    TreeNode<T> AddedNode = new TreeNode<T>()
                    {
                        Data = Items[i],
                        Parent = null,
                        Children = new List<TreeNode<T>>()
                    };//生成要添加的数据 

                    queue.Enqueue(AddedNode);//入队
                                             //看是否到了下一层的结点
                    if (lastNode != null &&
                        (compare(AddedNode.Data, lastNode.Data) == Tree.TeeNodeCompareResult.Child
                         || compare(AddedNode.Data, lastNode.Data) == Tree.TeeNodeCompareResult.NexLevelNode)//下一层：即结点是子结点或是下一层结点
                        )
                    {
                        currentNode = queue.Dequeue();

                    }
                    //找到对应的父结点
                    while (currentNode != null
                        &&
                        compare(AddedNode.Data, currentNode.Data) != TeeNodeCompareResult.Child
                        )
                    {
                        currentNode = queue.Dequeue();
                    }
                    if (currentNode != null && compare(AddedNode.Data, currentNode.Data) != TeeNodeCompareResult.EquealNode)
                    {
                        AddedNode.Parent = currentNode;
                        current = currentNode.Children;
                    }
                    current.Add(AddedNode);
                    lastNode = AddedNode;
                }
            }
            return result;
        }

        private int CompareResult(T ob1, T ob2) { switch (compare(ob1, ob2)) { case TeeNodeCompareResult.Child: case TeeNodeCompareResult.NextNode: case TeeNodeCompareResult.NexLevelNode: return 1; case TeeNodeCompareResult.Parent: case TeeNodeCompareResult.PreNode: return -1; default: return 0; } }
    }
}