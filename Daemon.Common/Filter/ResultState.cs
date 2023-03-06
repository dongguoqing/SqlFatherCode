using System.Collections.Generic;
using System;
namespace Daemon.Common.Filter
{
    public static class ResultState
    {
        public static string GetValue(int type)
        {
            List<string> list = new List<string>() { "操作失败！", "操作成功！", "数据未找到！" };
            return list[type];
        }
    }

    public enum State
    {
        Error = 0,
        Success = 1,
        NotFound = 2,
    }
}
