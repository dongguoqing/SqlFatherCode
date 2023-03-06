using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.SqlParser.Schema;

namespace Daemon.Common.SqlParser.Generator
{
    public class IncreaseDataGenerator:DataGenerator
    {
        public List<string> doGenerate(Field field, int rowNum)
        {
            String mockParams = field.MockParams;
            List<string> list = new List<string>(rowNum);
            if (string.IsNullOrEmpty(mockParams))
            {
                mockParams = "1";
            }
            int initValue = Int32.Parse(mockParams);
            for (int i = 0; i < rowNum; i++)
            {
                list.Add((initValue + i).ToString());
            }
            return list;
        }
    }
}