namespace Daemon.Common.Query.Framework.Filters.Operators
{
    public enum DateFilterOperator
    {
        EqualTo,
        After,
        GreaterThan,
        OnOrAfter,
        GreaterThanOrEqualTo,
        Before,
        LessThan,
        OnOrBefore,
        LessThanOrEqualTo,
        IsNull,
        IsNotNull,
        NotEqualTo,
        NotSupported,
        IsWithIn,
    }

    public class DateFilterOperatorAdapter
    {
        public static DateFilterOperator AdapterToOperator(string filterOperator)
        {
            switch (filterOperator.ToLower().Trim())
            {
                case ">":
                    return DateFilterOperator.GreaterThan;
                case ">=":
                    return DateFilterOperator.GreaterThanOrEqualTo;
                case "<":
                    return DateFilterOperator.LessThan;
                case "<=":
                    return DateFilterOperator.LessThanOrEqualTo;
                case "=":
                    return DateFilterOperator.EqualTo;
                case "<>":
                    return DateFilterOperator.NotEqualTo;
                case "is null":
                    return DateFilterOperator.IsNull;
                case "is not null":
                    return DateFilterOperator.IsNotNull;
                default:
                    return DateFilterOperator.NotSupported;
            }
        }
    }
}
