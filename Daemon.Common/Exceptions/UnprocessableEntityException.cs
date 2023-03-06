namespace Daemon.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    [Serializable]
    public class UnprocessableEntityException : Exception
    {
        private const string DEFAULT_MESSAGE = "The request was unable to be followed due to semantic errors.";

        public UnprocessableEntityException()
            : base(DEFAULT_MESSAGE)
        {
            Init();
        }

        public UnprocessableEntityException(string message)
            : base(message)
        {
            Init();
        }

        public UnprocessableEntityException(string message, Exception inner)
            : base(message, inner)
        {
            Init();
        }

        // This constructor is needed for serialization.
        protected UnprocessableEntityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Init();
        }

        private void Init()
        {
            HResult = 422;
        }
    }
}
