using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.Enums;

namespace Daemon.Common.SqlParser.Generator
{
    public class DataGeneratorFactory
    {
        private static Dictionary<MockTypeEnum, DataGenerator> mockTypeDataGeneratorMap = new Dictionary<MockTypeEnum, DataGenerator>() {
             {MockTypeEnum.NONE, new DefaultDataGenerator()},
             {MockTypeEnum.FIXED, new FixedDataGenerator()},
             {MockTypeEnum.RANDOM, new RandomDataGenerator()},
             {MockTypeEnum.RULE, new RuleDataGenerator()},
             {MockTypeEnum.DICT, new DictDataGenerator()},
             {MockTypeEnum.INCREASE, new IncreaseDataGenerator()}
        };

        private DataGeneratorFactory()
        {
        }

        /**
    * 获取实例
    *
    * @param mockTypeEnum
    * @return
    */
        public static DataGenerator getGenerator(MockTypeEnum mockTypeEnum)
        {
            return mockTypeDataGeneratorMap[mockTypeEnum];
        }
    }
}