using System;
using System.Collections.Generic;
using Daemon.Data.Infrastructure.Auth;
namespace Daemon.Data.Infrastructure.Configuration
{
    /// <summary>
    /// Provides the information of current client context information.
    /// </summary>
    public interface IContextInfoProvider
    {
        string RequestOrigin { get; }

        string Version { get; }

        string SessionItemId { get; }

        DateTime? ClientDateTime { get; }

        IAuthInfo AuthInfo { get; }

        string IPAddress { get; }

        string Host { get; }

        string UserAgent { get; }

        IEnumerable<string> SelectFields { get; }

        string CurrentVersion { get; }

        string Prefix { get; }
    }
}
