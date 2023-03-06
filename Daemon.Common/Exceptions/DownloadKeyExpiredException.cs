using System;
namespace Daemon.Common.Exceptions
{
    /// <summary>
    /// down load key expired exception
    /// </summary>
    public class DownloadKeyExpiredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadKeyExpiredException"/> class.
        /// constructor
        /// </summary>
        public DownloadKeyExpiredException()
            : base("The download key is expired")
        {
        }
    }

    public class ApiNotSupportedException : NotSupportedException
    {
        public ApiNotSupportedException()
            : base("Legacy functionality is no longer supported by this API.")
        {
        }
    }
}
