using System;
using System.Collections.Generic;
namespace Daemon.Controllers.V1
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("blueearth/test")]
    public class TestController : ControllerBase
    {

        public TestController() => System.Console.WriteLine();

        [Route("test")]
        public ActionResult Test()
        {
           // (string name, int age, string address) = WhoamI();
            return Ok();
        }

        /// <summary>
        /// C# 7.0 tuple语法糖
        /// </summary>
        /// <param name="name"></param>
        /// <param name="age"></param>
        /// <param name="WhoamI("></param>
        /// <returns></returns>
        private (List<int> name, int age, string address) WhoamI(int a)
        {
            return (new List<int>(){}, 28, "湖北省黄冈市");
        }
    }
}
