using System;
using System.Runtime.Serialization;
namespace Daemon.Common.Exceptions
{
    [Serializable]
    public class ValueDuplicateException : Exception
    {
        private const string DEFAULT_MESSAGE = "The entity has existed.";

        public ValueDuplicateException()
            : base(DEFAULT_MESSAGE)
        {
            Init();
        }

        public ValueDuplicateException(string message)
            : base(message)
        {
            Init();
        }

        public ValueDuplicateException(string message, Exception inner)
            : base(message, inner)
        {
            Init();
        }

        // This constructor is needed for serialization.
        protected ValueDuplicateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Init();
        }

        private void Init()
        {
            HResult = 409;
        }
    }
}
