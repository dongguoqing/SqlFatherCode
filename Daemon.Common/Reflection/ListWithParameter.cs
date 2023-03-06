using System.Collections.Generic;
namespace Daemon.Common.Reflection
{
   public class ListWithParameter<T> : List<T>
    {
        public string ExpressionType { get; set; }

        public string Parameter { get; set; }

        public bool NotContains(T item)
        {
            return !Contains(item);
        }
    }
}
