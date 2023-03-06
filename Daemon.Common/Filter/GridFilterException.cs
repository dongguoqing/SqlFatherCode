namespace Daemon.Common.Filter
{
    using System;
    using System.Runtime.Serialization;
    [Serializable]
    public class GridFilterException : Exception
    {
        public GridFilterException()
        {
        }

        public GridFilterException(string message)
            : base(message)
        {
        }

        public GridFilterException(string message, Exception innerExcpetion)
            : base(message, innerExcpetion)
        {
        }

        // This constructor is needed for serialization.
        protected GridFilterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
