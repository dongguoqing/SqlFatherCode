using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Daemon.Common.Reflection;
using Daemon.Common.Query.ExpressionProviders;

namespace Daemon.Common.Query.Framework.Query
{
    public class QueryTranslator : ExpressionVisitor
    {
        private string _orderBy = string.Empty;
        private string _returnValue;
        private int? _skip = null;
        private int? _take = null;
        private string _whereClause = string.Empty;

        private enum ContainsType
        {
            Left,
            Right,
            Both,
        }

        public QueryTranslator()
        {
            _returnValue = string.Empty;
        }

        public string OrderBy
        {
            get
            {
                return _orderBy;
            }
        }

        public int? Skip
        {
            get
            {
                return _skip;
            }
        }

        public int? Take
        {
            get
            {
                return _take;
            }
        }

        public string WhereClause
        {
            get
            {
                return _whereClause;
            }
        }

        public string Translate(Expression expression)
        {
            Visit(expression);
            return _returnValue;
        }

        protected bool IsNullConstant(Expression exp)
        {
            return exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.AndAlso)
            {
                Match match = Regex.Match(b.ToString(), @".+Param_0\.([^ ]+) >= ([0-9-./:]+).+Param_0\.[^ ]+ < [0-9./:]+.+");
                if (match.Success)
                {
                    string numberStr = match.Groups[2].Value;
                    string memberName = match.Groups[1].Value;
                    decimal number;
                    if (decimal.TryParse(numberStr, out number))
                    {
                        int count = BitConverter.GetBytes(decimal.GetBits(number)[3])[2] - 1;
                        number = number + ((decimal)Math.Pow(10, -1 * count) / 2);
                        _returnValue = string.Format("Round([{0}],{1}) = {2}", memberName, count, Math.Round(number, count));
                        return b;
                    }
                }
            }

            Visit(b.Left);
            var leftValue = _returnValue;

            MethodCallExpression mce = b.Left as MethodCallExpression;
            if (mce != null && mce.Method.Name == "CompareTo")
            {
                _returnValue = leftValue;
                switch (b.NodeType)
                {
                    case ExpressionType.LessThan:
                        _returnValue = _returnValue.Replace("CompareTo", "<");
                        break;

                    case ExpressionType.LessThanOrEqual:
                        _returnValue = _returnValue.Replace("CompareTo", "<=");
                        break;

                    case ExpressionType.GreaterThan:
                        _returnValue = _returnValue.Replace("CompareTo", ">");
                        break;

                    case ExpressionType.GreaterThanOrEqual:
                        _returnValue = _returnValue.Replace("CompareTo", ">=");
                        break;
                }

                return b;
            }

            switch (b.NodeType)
            {
                case ExpressionType.And:
                    _returnValue = " AND ";
                    break;

                case ExpressionType.AndAlso:
                    _returnValue = " AND ";
                    break;

                case ExpressionType.Or:
                    _returnValue = " OR ";
                    break;

                case ExpressionType.OrElse:
                    _returnValue = " OR ";
                    break;

                case ExpressionType.Equal:
                    if (IsNullConstant(b.Right))
                    {
                        _returnValue = " IS ";
                    }
                    else
                    {
                        _returnValue = " = ";
                    }

                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(b.Right))
                    {
                        _returnValue = " IS NOT ";
                    }
                    else
                    {
                        _returnValue = " <> ";
                    }

                    break;

                case ExpressionType.LessThan:
                    _returnValue = " < ";
                    break;

                case ExpressionType.LessThanOrEqual:
                    _returnValue = " <= ";
                    break;

                case ExpressionType.GreaterThan:
                    _returnValue = " > ";
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    _returnValue = " >= ";
                    break;

                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }

            var midValue = _returnValue;

            Visit(b.Right);
            var rightValue = _returnValue;
            if ((b.NodeType == ExpressionType.Equal || b.NodeType == ExpressionType.NotEqual) && !IsNullConstant(b.Right))
            {
                rightValue = "'" + Regex.Replace(TrimRightValue(rightValue), "'", "''") + "'";
            }

            _returnValue = leftValue + midValue + rightValue;
            _returnValue = "(" + _returnValue + ")";
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;

            if (q == null && c.Value == null)
            {
                _returnValue = "NULL";
            }
            else if (q == null)
            {
                string parameter = null;
                Type t = c.Value.GetType();
                if (t.IsGenericType)
                {
                    if (t.GetGenericTypeDefinition() == typeof(ListWithParameter<>))
                    {
                        PropertyInfo info = t.GetProperty("Parameter");
                        parameter = info.GetValue(c.Value, null).ToString();
                    }
                }

                if (parameter == "IN")
                {
                    var type = c.Value.GetType().GetTypeInfo();
                    var listType = typeof(List<>);
                    if (type.GetGenericTypeDefinition() == listType)
                    {
                        var list = c.Value as IEnumerable;
                        if (list != null)
                        {
                            foreach (var s in list)
                            {
                                _returnValue += "'" + s + "'" + ",";
                            }
                        }

                        _returnValue = _returnValue.TrimEnd(',');
                    }
                }
                else
                {
                    switch (Type.GetTypeCode(c.Value.GetType()))
                    {
                        case TypeCode.Boolean:
                            _returnValue = (((bool)c.Value) ? 1 : 0).ToString();
                            break;

                        case TypeCode.String:
                        case TypeCode.DateTime:
                            _returnValue = "'" + c.Value.ToString() + "'";
                            break;

                        case TypeCode.Object:
                            if (c.Value.GetType().FullName.Contains("TimeSpan"))
                            {
                                _returnValue = "'" + c.Value.ToString() + "'";
                            }
                            else
                            {
                                throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                            }

                            break;
                        default:
                            _returnValue = c.Value.ToString();
                            break;
                    }
                }
            }

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                _returnValue = "[" + m.Member.Name + "]";
                return m;
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                Visit(m.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
                return m;
            }

            if (m.Method.Name == DateFilterExtensions.DATE_FILTER_ISWITHIN_NAME)
            {
                Visit(m.Arguments[0]);
                var leftValue = _returnValue;
                Visit(m.Arguments[1]);
                var rightValue = _returnValue;
                _returnValue = $"{leftValue} {DateFilterExtensions.DATE_FILTER_ISWITHIN_NAME} {rightValue}";
                return m;
            }

            if (m.Method.Name == "Contains" || m.Method.Name == "NotContains")
            {
                if ((m.Object as ConstantExpression) != null)
                {
                    string parameter = null;
                    ConstantExpression exp = m.Object as ConstantExpression;
                    Type t = exp.Value.GetType();
                    if (t.GetGenericTypeDefinition() == typeof(ListWithParameter<>))
                    {
                        PropertyInfo info = t.GetProperty("Parameter");
                        parameter = info.GetValue(exp.Value, null).ToString();
                    }

                    if (parameter == "IN")
                    {
                        _returnValue = string.Empty;
                        var rightValue = ConvertListRightValue(exp.Value as IList);
                        Visit(m.Arguments[0]);
                        var leftValue = _returnValue;
                        _returnValue = leftValue + (m.Method.Name == "NotContains" ? " NOT" : string.Empty) + " IN(" + rightValue + ")";
                    }

                    return m;
                }
                else
                {
                    if (m.Object == null && m.Arguments.Count == 2)
                    {
                        Visit(m.Arguments[0]);
                    }
                    else
                    {
                        Visit(m.Object);
                    }

                    var leftValue = _returnValue;
                    if (m.Object == null && m.Arguments.Count == 2)
                    {
                        Visit(m.Arguments[1]);
                    }
                    else
                    {
                        Visit(m.Arguments[0]);
                    }

                    var rightValue = _returnValue;
                    _returnValue = leftValue + (m.Method.Name == "NotContains" ? " NOT" : string.Empty) + " LIKE " + ConvertContainsRightValue(TrimRightValue(rightValue), ContainsType.Both);
                    return m;
                }
            }

            if (m.Method.Name == "StartsWith")
            {
                Visit(m.Object);
                var leftValue = _returnValue;
                Visit(m.Arguments[0]);
                var rightValue = _returnValue;
                _returnValue = leftValue + " LIKE " + ConvertContainsRightValue(TrimRightValue(rightValue), ContainsType.Right);
                return m;
            }

            if (m.Method.Name == "EndsWith")
            {
                Visit(m.Object);
                var leftValue = _returnValue;
                Visit(m.Arguments[0]);
                var rightValue = _returnValue;
                _returnValue = leftValue + " LIKE " + ConvertContainsRightValue(TrimRightValue(rightValue), ContainsType.Left);
                return m;
            }

            if (m.Method.Name == "Take")
            {
                if (ParseTakeExpression(m))
                {
                    Expression nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }

            if (m.Method.Name == "Skip")
            {
                if (ParseSkipExpression(m))
                {
                    Expression nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }

            if (m.Method.Name == "ToUpper")
            {
                return Visit(m.Object);
            }

            if (m.Method.Name == "CompareTo")
            {
                MethodCallExpression mce = m.Object as MethodCallExpression;
                if (mce != null)
                {
                    MemberExpression me = mce.Object as MemberExpression;
                    if (me != null)
                    {
                        Visit(me);
                        string actualLeft = _returnValue;
                        Visit(m.Arguments[0]);
                        string actualRight = _returnValue;

                        _returnValue = actualLeft + " CompareTo " + actualRight;
                        return m;
                    }
                }
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    Visit(u.Operand);
                    break;
                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }

            return u;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        private bool ParseSkipExpression(MethodCallExpression expression)
        {
            ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                _skip = size;
                return true;
            }

            return false;
        }

        private bool ParseTakeExpression(MethodCallExpression expression)
        {
            ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                _take = size;
                return true;
            }

            return false;
        }

        private string TrimRightValue(string rightValue)
        {
            rightValue = rightValue.EndsWith("'") ? rightValue.Remove(rightValue.Length - 1, 1) : rightValue;
            rightValue = rightValue.StartsWith("'") ? rightValue.Remove(0, 1) : rightValue;
            return rightValue;
        }

        private string ConvertContainsRightValue(string rightValue, ContainsType containsType)
        {
            if (rightValue.Contains("'"))
            {
                rightValue = Regex.Replace(rightValue, "'", "''");
            }

            var specialChars = new List<string>() { "%", "_", "[", "]" };
            string escape = string.Empty;
            if (specialChars.Any(r => rightValue.Contains(r)))
            {
                rightValue = Regex.Replace(rightValue, "/", "//");
                specialChars.ForEach(c =>
                {
                    var replace = c == "[" || c == "]" ? "\\" + c : c;
                    rightValue = Regex.Replace(rightValue, replace, "/" + c);
                });

                escape = " escape '/'";
            }

            switch (containsType)
            {
                case ContainsType.Left:
                    return "'%" + rightValue + "'" + escape;
                case ContainsType.Right:
                    return "'" + rightValue + "%'" + escape;
                default:
                    return "'%" + rightValue + "%'" + escape;
            }
        }

        private string ConvertListRightValue(IList list)
        {
            List<string> convertList = new List<string>();
            foreach (var item in list)
            {
                convertList.Add("'" + Regex.Replace(item.ToString(), "'", "''") + "'");
            }

            return string.Join(",", convertList);
        }
    }
}
