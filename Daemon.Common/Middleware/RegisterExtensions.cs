using System.Globalization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System;
using Daemon.Common.Json;
using Daemon.Common.CaptchaHelp;
using Autofac;
using AspectCore.Extensions.Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Daemon.Model;
using Daemon.Common.SqlParser;
using Microsoft.AspNetCore.Authorization;
using Daemon.Common.Executer;
using Daemon.Common.Cache;
using Daemon.Common.Handlers;
namespace Daemon.Common.Middleware
{
    public static class RegisterExtensions
    {
        public static IServiceCollection Register(this IServiceCollection services)
        {
            //AddSingleton :每次请求都会获取同一个实例
            //AddScoped:每次请求都会获取一个新的实例,同一个请求获取多次的实例相同
            //AddTransient:每次请求都会获取一个新的实例，即使一个请求获取多次也是不同的实例
            services.AddSingleton<IDelimitedListParser, JsonDelimitedListParser>();
            services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ICaptcha, Captcha>();
            services.AddTransient<RelationshipExecutor>();
            services.AddSingleton<IDaemonDistributedCache, DaemonDistributedCache>();
            services.AddTransient<ApiDBContent>();
            services.AddSingleton<ISqlDialect, MySQLDialect>();
            //DI handler process function
            services.AddSingleton<IAuthorizationHandler, PolicyHandler>();
            SetAllowAllOrigin(services);
            return services;
        }

        private static void SetAllowAllOrigin(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigin", builder =>
                {
                    builder.SetPreflightMaxAge(TimeSpan.FromSeconds(1800L));
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });
        }

        public static IServiceProvider RegisterAutofac(IServiceCollection services, IConfiguration configuration, IContainer container)
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.Populate(services);
            var module = new ConfigurationModule(configuration);
            builder.RegisterModule(module);
            container = builder.Build();
            return new AutofacServiceProvider(container);
        }
    }
}
