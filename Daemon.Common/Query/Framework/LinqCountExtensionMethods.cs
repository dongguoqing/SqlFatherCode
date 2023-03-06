using Daemon.Common.Middleware;
namespace Daemon.Common.Query.Framework
{
    public static class LinqCountExtensionMethods
    {
        private const string COUNT = "@count";

        public static bool IsGetCount()
        {
            string countValue = HttpContextHelper.Current.Request.Query[COUNT];
            return !string.IsNullOrEmpty(countValue) && bool.TryParse(countValue, out bool count) && count;
        }
    }
}
