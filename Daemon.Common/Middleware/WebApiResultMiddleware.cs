using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Daemon.Common.Middleware
{
    public class WebApiResultMiddleware : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            //根据实际需求进行具体实现
            if (context.Result is ResultModel)
            {
                var objectResult = context.Result as ResultModel;
                // var state = 0;
                // if (objectResult.state.ToString().ToLower() == "ok"  || objectResult.state.ToString().ToLower() == "success")
                //     state = (int)Enum.CodeEnum.请求成功;
                // if (objectResult.state.ToString().ToLower() == "error")
                //     state = (int)Enum.CodeEnum.系统异常;
                context.Result = new ObjectResult(new { StatusCode = objectResult._statusCode, Items = objectResult.obj, msg = objectResult.msg });
            }
        }
    }
}
