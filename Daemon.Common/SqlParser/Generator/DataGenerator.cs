using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daemon.Common.SqlParser.Schema;
using Daemon.Model;


namespace Daemon.Common.SqlParser.Generator
{
    public interface DataGenerator
    {
        /**
   * 生成
   *
   * @param field 字段信息
   * @param rowNum 行数
   * @return 生成的数据列表
   */
        List<string> doGenerate(Field field, int rowNum);
    }
}