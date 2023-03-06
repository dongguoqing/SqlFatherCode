using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
namespace Daemon.Common.Middleware
{
    public static class HttpClientFactoryHelper
    {
        public static IHttpClientFactory _httpClientFactory;
        internal static void Configure(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
    }
}
