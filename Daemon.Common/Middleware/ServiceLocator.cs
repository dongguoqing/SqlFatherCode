using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using Daemon.Infrustructure.Contract;
namespace Daemon.Common.Middleware
{
    public static class ServiceLocator
    {

        private static IServiceProvider _applicationServices;
        public static IApplicationBuilder UseServiceProvider(this IApplicationBuilder app)
        {
            _applicationServices = app.ApplicationServices;
            return app;
        }

        public static T Resolve<T>()
        {
            return _applicationServices.CreateScope().ServiceProvider.GetRequiredService<T>();
        }
    }
}
