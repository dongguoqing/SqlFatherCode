using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Common.Exceptions
{
    public class BusinessException : Exception
    {
        public Object obj { get; set; }

        public BusinessException(int hResult, string message, Object obj = null)
            : base(message)
        {
            base.HResult = hResult;
            this.obj = obj;
        }
        public BusinessException(string message, Object obj = null)
           : base(message)
        {
            base.HResult = 200;
            this.obj = obj;
        }
    }
}
