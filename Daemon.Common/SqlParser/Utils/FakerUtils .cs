using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Faker;
using Daemon.Common.Enums;
using System.Text;

namespace Daemon.Common.SqlParser.Utils
{
    public class FakerUtils
    {
        /**
         * 获取随机值
         *
         * @param randomTypeEnum
         * @return
         */
        public static string getRandomValue(MockParamsRandomTypeEnum randomTypeEnum)
        {
            StringBuilder str = new StringBuilder();
            char c;
            Random random = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < 4; i++)
            {
                c = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                str.Append(c);
            }
            switch (randomTypeEnum)
            {
                case MockParamsRandomTypeEnum.NAME:
                    return Faker.Name.Last();
                case MockParamsRandomTypeEnum.CITY:
                    return Faker.Address.City();
                case MockParamsRandomTypeEnum.EMAIL:
                    return Faker.Internet.Email();
                case MockParamsRandomTypeEnum.URL:
                    return Faker.Internet.Url();
                case MockParamsRandomTypeEnum.IP:
                    return Faker.Internet.DomainName();
                case MockParamsRandomTypeEnum.INTEGER:
                    return Faker.RandomNumber.Next().ToString();
                case MockParamsRandomTypeEnum.DECIMAL:
                    return decimal.Parse(Faker.RandomNumber.Next().ToString()).ToString();
                case MockParamsRandomTypeEnum.UNIVERSITY:
                    return Faker.Country.Name();
                case MockParamsRandomTypeEnum.DATE:
                    return Faker.Identification.DateOfBirth().ToString();
                case MockParamsRandomTypeEnum.TIMESTAMP:
                    var startTime = TimeZone.CurrentTimeZone.ToLocalTime(Faker.Identification.DateOfBirth());
                    return ((long)(DateTime.Now - startTime).TotalMilliseconds).ToString();
                case MockParamsRandomTypeEnum.PHONE:
                    return Faker.Phone.Number();
                default:
                    return str.ToString();
            }
        }
    }
}