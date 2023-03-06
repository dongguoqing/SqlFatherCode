using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.SqlParser.Schema;
using Daemon.Common.Enums.Extension;
using Daemon.Common.Enums;
using Daemon.Common.SqlParser.Generator;

namespace Daemon.Common.SqlParser.Builder
{
    public class DataBuilder
    {
        /**
    * 生成数据
    *
    * @param tableSchema
    * @param rowNum
    * @return
    */
        public static Dictionary<string, List<string>> generateData(TableSchema tableSchema, int rowNum)
        {
            List<Field> fieldList = tableSchema.FieldList;
            // 初始化结果数据
            Dictionary<string, List<string>> resultList = new Dictionary<string, List<string>>();
            // 依次生成每一列
            foreach (var field in fieldList)
            {
                MockTypeEnum mockTypeEnum = GetByMockType(field.MockType);
                DataGenerator dataGenerator = DataGeneratorFactory.getGenerator(mockTypeEnum);
                List<String> mockDataList = dataGenerator.doGenerate(field, rowNum);
                String fieldName = field.FieldName;
                // 填充结果列表
                if (mockDataList != null)
                {
                    resultList.Add(fieldName, mockDataList);
                }
            }
            return resultList;
        }

        private static MockTypeEnum GetByMockType(string mockType)
        {
            MockTypeEnum mockTypeEnum = MockTypeEnum.NONE;
            switch (mockType)
            {
                case "递增":
                    mockTypeEnum = MockTypeEnum.INCREASE;
                    break;
                case "不模拟":
                    mockTypeEnum = MockTypeEnum.NONE;
                    break;
                case "固定":
                    mockTypeEnum = MockTypeEnum.FIXED;
                    break;
                case "随机":
                    mockTypeEnum = MockTypeEnum.RANDOM;
                    break;
                case "规则":
                    mockTypeEnum = MockTypeEnum.RULE;
                    break;
                case "词库":
                    mockTypeEnum = MockTypeEnum.DICT;
                    break;
            }
            return mockTypeEnum;
        }
    }
}