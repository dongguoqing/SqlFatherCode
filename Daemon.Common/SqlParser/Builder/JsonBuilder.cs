using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace Daemon.Common.SqlParser.Builder
{
    public class JsonBuilder
    {
        public static string GetDataListJson(Dictionary<string, List<string>> dataList)
        {
            StringBuilder sbStr = new StringBuilder();
            var dataListKeys = dataList.Keys;
            if (dataList.Values.Any())
            {
                sbStr.AppendLine("[");
                int? valueLength = dataList.Values.ToList()[0]?.Count;
                for (int j = 0; j < valueLength; j++)
                {
                    sbStr.AppendLine("  {");
                    for (int i = 0; i < dataListKeys.Count; i++)
                    {
                        if (i < dataListKeys.Count - 1)
                        {
                            sbStr.AppendLine("  \"" + dataListKeys.ToList()[i] + "\":" + "\"" + dataList[dataListKeys.ToList()[i]][j] + "\",");
                        }
                        else if (i == dataListKeys.Count - 1)
                        {
                            sbStr.AppendLine("  \"" + dataListKeys.ToList()[i] + "\":" + "\"" + dataList[dataListKeys.ToList()[i]][j] + "\"");
                        }
                    }

                    if (j == valueLength - 1)
                    {
                        sbStr.AppendLine("  }");
                    }
                    else
                    {
                        sbStr.AppendLine("  },");
                    }
                }
            }

            sbStr.AppendLine("]");
            return sbStr.ToString();
        }
    }
}