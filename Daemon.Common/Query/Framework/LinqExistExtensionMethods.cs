using System.Web;
using Daemon.Common.Middleware;

namespace Daemon.Common.Query.Framework
{
    public static class LinqExistExtensionMethods
    {
        private const string EXIST = "@exist";

        public static bool IsGetExist()
        {
            string value = HttpContextHelper.Current.Request.Query[EXIST];
            return !string.IsNullOrEmpty(value) && bool.TryParse(value, out bool exist) && exist;
        }
    }
}
