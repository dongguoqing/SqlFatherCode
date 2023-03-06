namespace Daemon.Common.Query.Framework.Filters.Operators
{
    public enum StringFilterOperator
    {
        Contains,
        DoesNotContain,
        EqualTo,
        EndsWith,
        IsNull,
        IsNotNull,
        NotEqualTo,
        StartsWith,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        NotSupported,
    }

    public class StringFilterOperatorAdapter
    {
        public static StringFilterOperator AdapterToOperator(string filterOperator)
        {
            switch (filterOperator.ToLower().Trim())
            {
                case "like":
                    return StringFilterOperator.Contains;
                case "not like":
                    return StringFilterOperator.DoesNotContain;

                // case "is":
                case "=":
                    return StringFilterOperator.EqualTo;

                // case "is not":
                case "<>":
                    return StringFilterOperator.NotEqualTo;
                case "is null":
                    return StringFilterOperator.IsNull;
                case "is not null":
                    return StringFilterOperator.IsNotNull;
                case ">":
                    return StringFilterOperator.GreaterThan;
                case ">=":
                    return StringFilterOperator.GreaterThanOrEqual;
                case "<":
                    return StringFilterOperator.LessThan;
                case "<=":
                    return StringFilterOperator.LessThanOrEqual;
                default:
                    return StringFilterOperator.NotSupported;
            }
        }
    }
}
