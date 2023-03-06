using System;

namespace Daemon.Common.ExtensionMethod
{
    public static class StringExtensionMethod
    {
        public static bool NotContains(this string originalValue, string value)
        {
            return !originalValue.Contains(value);
        }

        public static bool IsGuid(this string rawData) =>
            !string.IsNullOrWhiteSpace(rawData)
            && rawData.Length > 1
            && rawData[0] == '_'
            && Guid.TryParse(rawData.Substring(1), out Guid _);
    }
}
