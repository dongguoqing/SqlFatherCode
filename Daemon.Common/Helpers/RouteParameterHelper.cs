namespace Daemon.Common.Helpers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	public class RouteParameterHelper
	{
		private const string ALL = "all";

		public static bool CheckRelationship<T>(string relationships, T e)
		{
			return relationships.Split(',').Any(r => r.Equals(ALL, StringComparison.OrdinalIgnoreCase) || r.Equals(e.ToString(), StringComparison.OrdinalIgnoreCase));
		}

		public static bool CheckRouteParameter<T>(IEnumerable<string> parameterValues, T e)
		{
			return parameterValues.Any(r => r.Equals(ALL, StringComparison.OrdinalIgnoreCase) || r.Equals(e.ToString(), StringComparison.OrdinalIgnoreCase));
		}
	}
}
