using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Daemon.Model;
namespace Daemon.Common.Middleware
{
    public static class DbContextExtensions
    {
        public static IApplicationBuilder UseStaticDbContext(this IApplicationBuilder app){
            var dbContext = app.ApplicationServices.GetRequiredService<ApiDBContent>();
            DbContextHelper.Configure(dbContext);
            return app;
        }
    }
}   
