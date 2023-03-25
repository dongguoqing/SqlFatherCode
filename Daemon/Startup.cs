using System.ComponentModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using Daemon.Model;
using Microsoft.EntityFrameworkCore;
using Autofac;
using Daemon.Common.Middleware;

namespace Daemon
{
    public class Startup
    {
        private string connection = string.Empty;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("autofac.json");
            connection = configuration.GetSection("AppSettings:DefaultConnection").Value;
            Configuration = builder.Build();
        }
    
        public IConfiguration Configuration { get; }
        public Autofac.IContainer Container { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApiDBContent>(option =>
            {
                //option.UseSqlite("Filename=./learn01.db");
                option.UseMySql(connection, Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.27-log"));
            });
            services.AddSingleton<HttpContextAccessor>();
            services.AddSingleton<DbContext>(provider => provider.GetService<ApiDBContent>());
            services.AddDaemonProvider(Configuration, 2, true, false, false, false);
            services.AddControllers();
            services.AddConfigCors(Configuration);
            services.AddJwtBearExt(Configuration);
            services.Register();
            return RegisterExtensions.RegisterAutofac(services, Configuration, this.Container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //Enable Authentication
            app.UseAuthentication();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticHttpContext().UseServiceProvider();
            app.UseStaticDbContext();
            app.UseCors("qwer");
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSwaggerUI();

           // app.SetFileExtensionContentType();

            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
