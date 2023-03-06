namespace Daemon.Common.Executer
{
    using Daemon.Model.Entities;
    using System.Collections.Generic;
    using Daemon.Common.Middleware;
    using System.Linq;
    public class RelationshipExecutor
    {
        private const char LEFT_PARENTHESIS = '(';
        private const char RIGHT_PARENTHESIS = ')';
        private const char COMMA = ',';
        private const char AND = '&';

        private List<Relationship> _relationships = null;

        public List<Relationship> GetRelationships()
        {
            if (_relationships == null)
            {
                if (HttpContextHelper.Current == null)
                {
                    return _relationships;
                }
                string value = HttpContextHelper.Current.Request.Query["@relationships"].ToArray().ToList().FirstOrDefault();
                _relationships = ParseRelationships(value);
            }

            return _relationships;
        }

        private List<Relationship> ParseRelationships(string relationshipString)
        {
            if (string.IsNullOrWhiteSpace(relationshipString))
            {
                return null;
            }

            List<Relationship> relationships = new List<Relationship>();
            while (!string.IsNullOrEmpty(relationshipString))
            {
                relationshipString = relationshipString.TrimStart();
                string value = GetMatchValue(relationshipString, COMMA);
                if (!string.IsNullOrEmpty(value))
                {
                    relationshipString = relationshipString.Substring(value.Length);
                    value = value.TrimEnd(COMMA);
                    relationships.Add(CreateRelationship(value));
                }
            }

            return relationships;
        }

        private Relationship CreateRelationship(string value)
        {
            Relationship relationship = new Relationship();
            if (value.IndexOf(LEFT_PARENTHESIS) < 0)
            {
                relationship.Name = value;
            }
            else
            {
                int firstLeftParenthesisIndex = value.IndexOf(LEFT_PARENTHESIS);
                relationship.Name = value.Substring(0, firstLeftParenthesisIndex);
                string parameters = value.Substring(firstLeftParenthesisIndex + 1, value.Length - firstLeftParenthesisIndex - 2);

                while (!string.IsNullOrEmpty(parameters))
                {
                    parameters = parameters.TrimStart();
                    string parameter = GetMatchValue(parameters, AND);
                    if (!string.IsNullOrEmpty(parameter))
                    {
                        parameters = parameters.Substring(parameter.Length);
                        parameter = parameter.Trim(AND);
                        int firstEqualSignIndex = parameter.IndexOf('=');
                        if (firstEqualSignIndex < 0)
                        {
                            continue;
                        }

                        string parameterName = parameter.Substring(0, firstEqualSignIndex);
                        string parameterValue = parameter.Substring(firstEqualSignIndex + 1, parameter.Length - firstEqualSignIndex - 1);
                        switch (parameterName)
                        {
                            case "@fields":
                                relationship.SelectFields = parameterValue;
                                break;
                            case "@relationships":
                                relationship.Relationships = ParseRelationships(parameterValue);
                                break;
                        }
                    }
                }
            }

            return relationship;
        }

        private string GetMatchValue(string relationshipString, char endSymbol)
        {
            int num = 0;
            string value = string.Empty;
            foreach (var word in relationshipString)
            {
                value += word;
                if (word == LEFT_PARENTHESIS)
                {
                    num++;
                }
                else if (word == RIGHT_PARENTHESIS)
                {
                    num--;
                }
                else if (word == endSymbol)
                {
                    if (num == 0)
                    {
                        break;
                    }
                }
            }

            return value;
        }
    }
}
