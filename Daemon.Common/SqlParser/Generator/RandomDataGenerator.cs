using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.SqlParser.Schema;
using Daemon.Common.Enums;
using Daemon.Common.SqlParser.Utils;

namespace Daemon.Common.SqlParser.Generator
{
    public class RandomDataGenerator : DataGenerator
    {
        public List<String> doGenerate(Field field, int rowNum)
        {
            String mockParams = field.MockParams;
            List<String> list = new List<String>(rowNum);
            for (int i = 0; i < rowNum; i++)
            {
                MockParamsRandomTypeEnum randomTypeEnum = GetByMockParams(field.MockParams);
                String randomString = FakerUtils.getRandomValue(randomTypeEnum);
                list.Add(randomString);
            }
            return list;
        }

        private MockParamsRandomTypeEnum GetByMockParams(string mockParams)
        {
            switch (mockParams)
            {
                case "字符串":
                    return MockParamsRandomTypeEnum.STRING;
                case "人名":
                    return MockParamsRandomTypeEnum.NAME;
                case "城市":
                    return MockParamsRandomTypeEnum.CITY;
                case "网址":
                    return MockParamsRandomTypeEnum.URL;
                case "邮箱":
                    return MockParamsRandomTypeEnum.EMAIL;
                case "IP":
                    return MockParamsRandomTypeEnum.IP;
                case "整数":
                    return MockParamsRandomTypeEnum.INTEGER;
                case "小数":
                    return MockParamsRandomTypeEnum.DECIMAL;
                case "大学":
                    return MockParamsRandomTypeEnum.UNIVERSITY;
                case "日期":
                    return MockParamsRandomTypeEnum.DATE;
                case "时间戳":
                    return MockParamsRandomTypeEnum.TIMESTAMP;
                case "手机号":
                    return MockParamsRandomTypeEnum.PHONE;
                default:
                    return MockParamsRandomTypeEnum.STRING;
            }
        }
    }
}