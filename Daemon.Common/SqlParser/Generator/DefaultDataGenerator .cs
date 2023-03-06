using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.SqlParser.Schema;

namespace Daemon.Common.SqlParser.Generator
{
    public class DefaultDataGenerator : DataGenerator
    {
        public List<string> doGenerate(Field field, int rowNum)
        {
            string mockParams = field.MockParams;
            List<string> list = new List<string>(rowNum);
            // 主键采用递增策略
            if (field.PrimaryKey)
            {
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
            // 使用默认值
            string defaultValue = field.DefaultValue;
            // 特殊逻辑，日期要伪造数据
            if ("CURRENT_TIMESTAMP".Equals(defaultValue))
            {
                defaultValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            for (int i = 0; i < rowNum; i++)
            {
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    list.Add(defaultValue);
                }
                else
                {
                    list.Add("");
                }
            }
            return list;
        }
    }
}