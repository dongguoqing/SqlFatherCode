namespace Daemon.Common.Query.Framework.Filters.Operators
{
    public enum NumericFilterOperator
    {
        EqualTo,
        GreaterThan,
        GreaterThanOrEqualTo,
        LessThan,
        LessThanOrEqualTo,
        IsNull,
        IsNotNull,
        NotEqualTo,
        NotSupported,
    }

    public class NumericFilterOperatorAdapter
    {
        public static NumericFilterOperator AdapterToOperator(string filterOperator)
        {
            switch (filterOperator.ToLower().Trim())
            {
                case ">":
                    return NumericFilterOperator.GreaterThan;
                case ">=":
                    return NumericFilterOperator.GreaterThanOrEqualTo;
                case "<":
                    return NumericFilterOperator.LessThan;
                case "<=":
                    return NumericFilterOperator.LessThanOrEqualTo;
                case "=":
                    return NumericFilterOperator.EqualTo;
                case "<>":
                    return NumericFilterOperator.NotEqualTo;
                case "is null":
                    return NumericFilterOperator.IsNull;
                case "is not null":
                    return NumericFilterOperator.IsNotNull;
                default:
                    return NumericFilterOperator.NotSupported;
            }
        }
    }
}
