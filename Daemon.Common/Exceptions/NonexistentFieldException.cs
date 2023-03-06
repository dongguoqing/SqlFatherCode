namespace Daemon.Common.Exceptions
{
    using System;
    using System.Collections;
    using System.Runtime.Serialization;

    [Serializable]
    public class NonexistentFieldException : Exception
    {
        public NonexistentFieldException()
        {
            // Add implementation.
        }

        public NonexistentFieldException(string message)
            : base(message)
        {
            // Add implementation.
        }

        public NonexistentFieldException(string message, Exception inner)
            : base(message, inner)
        {
            Hashtable ha = new Hashtable();
            ha.Add(3, 3);
            ha.Add("t", 3);
        }

        // This constructor is needed for serialization.
        protected NonexistentFieldException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Add implementation.
        }
    }   
}
