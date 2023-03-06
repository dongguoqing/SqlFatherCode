using System.Threading;
using Daemon.Common;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
namespace Daemon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.Build();
            string ip = config["ip"] == null ? "*" : config["ip"];
            string port = config["port"] == null ? ConfigurationManager.AppSettings["port"] : config["port"];
            return WebHost.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
            {

            })
                .UseStartup<Startup>().UseUrls($"http://{ip}:{port}").UseKestrel(option =>
                {
                    option.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(20);
                    option.Limits.MaxRequestBodySize = int.MaxValue;
                    option.Limits.MaxRequestBufferSize = int.MaxValue;
                    option.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(20);
                }).ConfigureAppConfiguration((hostingContext, myConfig) =>
                {
                    //myConfig.AddCommandLine(args);
                });
        }
    }
}
