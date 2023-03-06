using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.SqlParser.Schema;


namespace Daemon.Common.SqlParser.Generator
{
    public class FixedDataGenerator : DataGenerator
    {
        public List<string> doGenerate(Field field, int rowNum)
        {
            string mockParams = field.MockParams;
            if (string.IsNullOrEmpty(mockParams))
            {
                mockParams = "6";
            }
            List<String> list = new List<string>(rowNum);
            for (int i = 0; i < rowNum; i++)
            {
                list.Add(mockParams);
            }
            return list;
        }
    }
}