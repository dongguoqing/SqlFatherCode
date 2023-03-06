using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.SqlParser.Schema;
using Daemon.Common.Exceptions;
using Daemon.Common.Enums;
using Daemon.Common.Middleware;
using Daemon.Service.Contract;
using Daemon.Model;

namespace Daemon.Common.SqlParser.Generator
{
    public class DictDataGenerator : DataGenerator
    {
        public List<String> doGenerate(Field field, int rowNum)
        {
            string mockParams = field.MockParams;
            int id = Int32.Parse(mockParams);
            Dict dict = ServiceLocator.Resolve<IDictService>().FindById(id);
            if (dict == null)
            {
                throw new BusinessException((int)ErrorCode.NOT_FOUND_ERROR, "词库不存在");
            }
            List<string> wordList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(dict.Content);
            List<String> list = new List<String>(rowNum);
            Random r = new Random();
            for (int i = 0; i < rowNum; i++)
            {
                String randomStr = wordList[r.Next(0, wordList.Count())];
                list.Add(randomStr);
            }
            return list;
        }
    }
}