using System.Collections.Generic;
namespace Daemon.Common.Const
{
    public static class FilterConst
    {
        public const string FILTER_OPERATION_EQUAL_TO = "EqualTo";

        public static Dictionary<string, Dictionary<string, object>> FilterFields { get; set; } =
            new Dictionary<string, Dictionary<string, object>>()
            {
                { "Gender", Genders },
            };

        public static Dictionary<string, string> ConvertOperations { get; set; } = new Dictionary<string, string>()
        {
            { "contains", "Contains" },
            { "notcontains", "DoesNotContain" },
            { "eq", "EqualTo" },
            { "noteq", "NotEqualTo" },
            { "endswith", "EndsWith" },
            { "isnull", "IsNull" },
            { "isnotnull", "IsNotNull" },
            { "ne", "NotEqualTo" }, 
            { "startswith", "StartsWith" },
            { "gt", "GreaterThan" },
            { "ge", "GreaterThanOrEqualTo" },
            { "lt", "LessThan" },
            { "le", "LessThanOrEqualTo" },
            { "after", "After" },
            { "onorafter", "OnOrAfter" },
            { "before", "Before" },
            { "onorbefore", "OnOrBefore" },
            { "in", "In" },
        };

        private static readonly Dictionary<string, object> Genders = new Dictionary<string, object>()
        {
            { "Female", "F" },
            { "Male", "M" },
        };
    }
}
