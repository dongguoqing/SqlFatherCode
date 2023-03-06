namespace Daemon.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    [Serializable]
    public class BadRequestException : Exception
    {
        private const string DEFAULT_MESSAGE = "The server cannot or will not process the request due to an apparent client error.";

        public BadRequestException()
            : base(DEFAULT_MESSAGE)
        {
            Init();
        }

        public BadRequestException(string message)
            : base(message)
        {
            Init();
        }

        public BadRequestException(string message, Exception inner)
            : base(message, inner)
        {
            Init();
        }

        // This constructor is needed for serialization.
        protected BadRequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Init();
        }

        private void Init()
        {
            HResult = 400;
        }
    }
}
