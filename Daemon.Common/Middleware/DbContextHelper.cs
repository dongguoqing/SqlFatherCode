
using System.Net.Http;
using Daemon.Model;
namespace Daemon.Common.Middleware
{
    public static class DbContextHelper
    {
        public static ApiDBContent _Context;
        internal static void Configure(ApiDBContent context)
        {
            _Context = context;
        }
    }
}
