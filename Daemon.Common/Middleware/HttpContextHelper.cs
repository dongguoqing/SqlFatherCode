using System;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Daemon.Model;
namespace Daemon.Common.Middleware
{
    public static class HttpContextHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;
        public static Microsoft.AspNetCore.Http.HttpContext Current => _httpContextAccessor.HttpContext;
        public static void Configure(IHttpContextAccessor contextAccessor)
        {
            _httpContextAccessor = contextAccessor;
        }
    }
}
