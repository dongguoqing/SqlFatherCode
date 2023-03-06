using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.SqlParser.Schema;
using System.Text.RegularExpressions;
using Fare;

namespace Daemon.Common.SqlParser.Generator
{
    public class RuleDataGenerator:DataGenerator
    {
        public List<string> doGenerate(Field field, int rowNum)
        {
            string mockParams = field.MockParams;
            List<string> list = new List<string>(rowNum);
            Xeger regex = new Xeger(mockParams);
            for (int i = 0; i < rowNum; i++)
            {
                string randomStr = regex.Generate();
                list.Add(randomStr);
            }
            return list;
        }
    }
}