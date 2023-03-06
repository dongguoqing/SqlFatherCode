using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System;
namespace Daemon.Extensions
{
    public static class HttpContextExtension
    {
        public static string GetClientIPAddress(this HttpRequest request)
        {
            var host = request.Host;
            var ipAddress = string.Join(':', new string[] { host.Host, host.Port.ToString() });
            string realIpBeforeProxy = null;
            var separator = ",";
            var colon = ":";
            var brakets = new char[] { '[', ']' };
            var fowardedIps = string.Join(separator, request.Headers["X-Forwarded-For"].ToList() ?? new List<string>());
            if (!string.IsNullOrWhiteSpace(fowardedIps))
            {
                // x-forwarded will be like "client1, proxy1, proxy2"
                var ips = fowardedIps.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                var clientIpPattern = ips.FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(clientIpPattern))
                {
                    /*
					 * currently we used AWS LB
					 * please reference https://docs.aws.amazon.com/elasticloadbalancing/latest/application/x-forwarded-headers.html#x-forwarded-for
					 * x-forwarded-for may include port
					 */
                    var colonPosition = clientIpPattern.LastIndexOf(colon);

                    // proxy with port found, remove port part
                    if (colonPosition != -1)
                    {
                        clientIpPattern = clientIpPattern.Substring(0, colonPosition);
                    }

                    // trim out bracket if ipv6
                    realIpBeforeProxy = clientIpPattern.Trim(brakets);
                }
            }

            return realIpBeforeProxy ?? ipAddress;
        }
    }
}
