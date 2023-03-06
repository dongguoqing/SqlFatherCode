using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
namespace Daemon.Common.Middleware
{
    public static class HttpClientFactoryExtensions
    {
        public static IApplicationBuilder UseStaticHttpClientFactory(this IApplicationBuilder app)
        {
            var httpClientFactory = app.ApplicationServices.GetRequiredService<IHttpClientFactory>();
            HttpClientFactoryHelper.Configure(httpClientFactory);
            return app;
        }
    }
}
