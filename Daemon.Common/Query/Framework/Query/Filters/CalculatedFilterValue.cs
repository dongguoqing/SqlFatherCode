using Daemon.Common.Query.Framework.Filters;
namespace Daemon.Common.Query.Framework.Query
{
    public class CalculatedFilterValue
    {
        public CalculatedFilterValue(SerializableCalculatedFilterValue calculatedFilterValue)
        {
            Calculation = calculatedFilterValue.Calculation;
            MathOperation = calculatedFilterValue.MathOperation;
            Value = calculatedFilterValue.Value;
        }

        public string Calculation { get; private set; }

        public string MathOperation { get; private set; }

        public string Value { get; private set; }
    }
}
