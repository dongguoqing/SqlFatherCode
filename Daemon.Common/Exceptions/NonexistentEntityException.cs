namespace Daemon.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    [Serializable]
    public class NonexistentEntityException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NonexistentEntityException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception.</param>
        public NonexistentEntityException(string message = "nonexistent entity", Exception inner = null)
            : base(message, inner)
        {
        }

        // This constructor is needed for serialization.
        protected NonexistentEntityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
