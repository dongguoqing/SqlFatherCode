namespace Daemon.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    [Serializable]
    public class PreconditionFailedException : Exception
    {
        private const string DEFAULT_MESSAGE = "Precondition Failed.";

        public PreconditionFailedException()
            : base(DEFAULT_MESSAGE)
        {
            Init();
        }

        public PreconditionFailedException(string message)
            : base(message)
        {
            Init();
        }

        public PreconditionFailedException(string message, Exception inner)
            : base(message, inner)
        {
            Init();
        }

        // This constructor is needed for serialization.
        protected PreconditionFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Init();
        }

        private void Init()
        {
            HResult = 412;
        }
    }
}
