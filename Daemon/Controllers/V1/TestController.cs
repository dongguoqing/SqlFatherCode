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

        [HttpGet,Route("test")]
        public ActionResult Test()
        {
            DateTime localDateTime = DateTime.Parse("2023-03-24 20:00:00");
            TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            var time11 = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse("2023-03-24 09:00:00"), "Eastern Standard Time");
            var c1 = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(time11, DateTimeKind.Unspecified), localTimeZone);
            var time22 = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Parse("2023-03-24 13:00:00"), "Eastern Standard Time");
            var c2 = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(DateTime.Parse("2023-03-24 10:56:07"), DateTimeKind.Unspecified), localTimeZone);
            Console.WriteLine("c1:" + c1);
            Console.WriteLine("c2:" + c2);
            Console.WriteLine("c3:" + TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(DateTime.Parse("2023-03-24 09:00:00"), DateTimeKind.Unspecified), localTimeZone));

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
