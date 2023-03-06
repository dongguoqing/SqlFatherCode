namespace Daemon.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    [Serializable]
    public class TokenExpiredException: Exception
    {
        public TokenExpiredException(string message = "Token is expired ", Exception inner = null)
          : base(message, inner)
        {
        }
    }
}
