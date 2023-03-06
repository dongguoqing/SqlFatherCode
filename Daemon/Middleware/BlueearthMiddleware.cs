
using System.Runtime.InteropServices.ComTypes;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Daemon.Common.Filter;
using Daemon.Common.Middleware.Filter;
using Microsoft.AspNetCore.Mvc;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NLog;
using CSRedis;
using Daemon.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Daemon.Repository.Contract;
using Daemon.Common.Exceptions;
using Daemon.Data.Substructure.Helpers;

namespace Daemon.Common.Middleware
{
    public static class DaemonMiddleware
    {
        public static IContainer Container { get; private set; }
        private static IConfiguration _configuration;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="Configuration"></param>
        /// <param name="isToLower">是否转小写</param>
        /// <param name="requestLength">外部请求地址的个数</param>
        /// <returns></returns>
        public static IServiceCollection AddDaemonProvider(this IServiceCollection services, IConfiguration Configuration, int requestLength, bool isToLower = true, bool isAuth = true, bool isMq = false, bool isRedis = false)
        {
            services.InitCap(isMq, Configuration);

            // InitRedis(isRedis, Configuration);

            var mvc = services.AddMvc(options =>
            {
                if (isAuth)
                    options.Filters.Add(typeof(DaemonAuthFilter));
                options.Filters.Add(typeof(RequestModelValidator));
                options.Filters.Add(typeof(CustomExceptionFilterAttribute));
                options.Filters.Add(typeof(WebApiResultMiddleware));
                options.RespectBrowserAcceptHeader = true;
                //options.UseCentralRoutePrefix(new RouteAttribute("api/v{version}"));
                //options.Conventions.Add(new NamespaceRoutingConvention("My Application Description"));
            });

            services.FormatLowerContractResolver(isToLower);


            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
            });
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressConsumesConstraintForFormFileParameters = true;
                options.SuppressInferBindingSourcesForParameters = true;
                options.SuppressModelStateInvalidFilter = false;
                //设置实体验证统一返回的数据格式
                options.SetErrorResponse();
                //禁用默认行为
                options.SetInvalidModelStateResponse();
            });

            services.AddSwagger();
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        private static void BindForNotificationHelper()
        {
            NotificationHelper.Instance.OnEntitiesAdded = (entities) =>
              {
                  NotifyScheduleDataChanged(entities);
              };

            NotificationHelper.Instance.OnEntitiesChanged = (entities) =>
            {
                NotifyScheduleDataChanged(entities);
            };

            NotificationHelper.Instance.OnEntitiesDeleted = (entities) =>
            {
                NotifyScheduleDataChanged(entities);
            };
        }

        private void NotifyScheduleDataChanged(Dictionary<Type, List<object>> entities)
        {
            var typeMap = new Dictionary<Type, ScheduleUpdateType>()
            {
                { typeof(Staff), ScheduleUpdateType.Technician },
                { typeof(WorkOrder), ScheduleUpdateType.WorkOrder },
                { typeof(WorkTemplate), ScheduleUpdateType.Service },
                { typeof(EquipmentWorkItem), ScheduleUpdateType.Service },
                { typeof(StaffRestriction), ScheduleUpdateType.Restriction },
            };

            if (entities.Count > 0)
            {
                var entityType = entities.Keys.ElementAt(0);

                if (typeMap.TryGetValue(entityType, out var scheduleType))
                {
                    ScheduleHub.NotifyScheduleChanged(scheduleType, null, null);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="isMq"></param>
        /// <param name="Configuration"></param>
        private static void InitCap(this IServiceCollection services, bool isMq, IConfiguration Configuration)
        {
            if (!isMq)
            {
                return;
            }

            services.AddCap(x =>
             {

                 x.UseEntityFramework<ApiDBContent>();
                 x.UseRabbitMQ(options =>
                 {
                     options.HostName = Configuration.GetSection("AppSettings:rabbitServer").Value;
                     //options.Port = Convert.ToInt32 (ConfigurationManager.AppSettings["rabbitServerPort"]);
                     options.UserName = Configuration.GetSection("AppSettings:rabbitServerUserName").Value;
                     options.Password = Configuration.GetSection("AppSettings:rabbitServerPassword").Value;
                 });
                 x.SucceedMessageExpiredAfter = 24 * 3600;
                 x.FailedRetryCount = 10;
                 x.FailedRetryInterval = 2;
             });
        }

        /// <summary>
        /// 
        /// </summary>
        private static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(m =>
            {
                m.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "DaemonSwagger", Version = "V1" });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isRedis"></param>
        /// <param name="Configuration"></param>
        private static void InitRedis(bool isRedis, IConfiguration Configuration)
        {
            if (!isRedis)
            {
                return;
            }

            var redisConStr = Configuration.GetSection("AppSettings:RedisAddress")?.Value;
            CSRedisClient csredis = null;
            if (Configuration.GetSection("AppSettings:Sentinel").Value != null || (Configuration.GetSection("AppSettings:Sentinel:0").Value != "" && Configuration.GetSection("AppSettings:Sentinel:0").Value != null))
            {
                if (Configuration.GetSection("AppSettings:Sentinel").Get<string[]>().Length > 0)
                {
                    try
                    {
                        csredis = new CSRedisClient(Configuration.GetSection("AppSettings:RedisAddress").Value.ToString(), Configuration.GetSection("AppSettings:Sentinel").Get<string[]>());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    csredis = new CSRedisClient(redisConStr);
                }
            }
            else
                csredis = new CSRedisClient(redisConStr);
            Console.WriteLine("链接redis完毕");
            RedisHelper.Initialization(csredis);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        private static void SetErrorResponse(this ApiBehaviorOptions options)
        {
            options.InvalidModelStateResponseFactory = (context) =>
            {
                var errors = context.ModelState.Values.SelectMany(x => x.Errors.Select(p => p.ErrorMessage)).ToList();
                throw new BadRequestException();
            };
        }

        /// <summary>
        ///
        /// </summary>
        private static void SetInvalidModelStateResponse(this ApiBehaviorOptions options)
        {
            options.InvalidModelStateResponseFactory = (context) =>
            {
                if (context.ModelState.IsValid)
                    return null;
                var error = "";
                foreach (var item in context.ModelState)
                {
                    var state = item.Value;
                    var message = state.Errors.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.ErrorMessage))?.ErrorMessage;
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        message = state.Errors.FirstOrDefault(o => o.Exception != null)?.Exception.Message;
                    }
                    if (string.IsNullOrWhiteSpace(message))
                        continue;
                    error = message;
                    break;
                }
                throw new BadRequestException();
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private static void FormatLowerContractResolver(this IServiceCollection services, bool isToLower)
        {
            if (isToLower)
            {
                services.AddControllers().AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new LowercaseContractResolver();
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd";
                });
            }
            else
            {
                services.AddControllers().AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });
            };
        }

        public static void UseSwaggerUI(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(m =>
            {
                m.SwaggerEndpoint("/swagger/v1/swagger.json", "DaemonSwagger");
            });
        }

        public static void AddConfigCors(this IServiceCollection service, IConfiguration Configuration)
        {
            service.AddCors(option =>
            {
                option.AddPolicy("qwer", policy =>
                {
                    string corsUrl = Configuration.GetSection("CorsOrigins")?.Value;
                    string[] codeArray = corsUrl.Split(",");
                    policy.WithOrigins(codeArray).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            });
        }

        public static void SetFileExtensionContentType(this IApplicationBuilder app)
        {
            //特殊格式文件需要访问到的话 需要配置一下
            var provider = new FileExtensionContentTypeProvider();
            // provider.Mappings[".glb"] = "application/octet-stream";
            // provider.Mappings[".apk"] = "applicationnd.android.package-archive";
            // provider.Mappings[".rvt"] = "application/octet-stream";
            // provider.Mappings[".ifc"] = "application/octet-stream";
            // provider.Mappings[".json"] = "application/octet-stream";
            // provider.Mappings[".be2x"] = "application/octet-stream";
            // provider.Mappings[".be2m"] = "application/octet-stream";
            // provider.Mappings[".dwg"] = "application/octet-stream";
            // provider.Mappings[".dwt"] = "application/octet-stream";
            // provider.Mappings[".dxf"] = "application/octet-stream";
            // provider.Mappings[".i3dm"] = "application/octet-stream";
            // provider.Mappings[".b3dm"] = "application/octet-stream";
            // provider.Mappings[".3dm"] = "application/octet-stream";
            // provider.Mappings[".mpp"] = "application/octet-stream";
            // provider.Mappings[".ceb"] = "ceb/files";
            var sysInfoRepository = ServiceLocator.Resolve<IBlogSysInfoRepository>();
            var fileAddress = sysInfoRepository.FindAll().FirstOrDefault(r => r.InfoId == "fileaddress");
            string[] fileAddressArray = fileAddress?.InfoValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < fileAddressArray.Length; i++)
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    //配置除了默认的wwwroot文件中的静态文件以外的文件夹  提供 Web 根目录外的文件  经过此配置以后，就可以访问StaticFiles文件下的文件
                    FileProvider = new PhysicalFileProvider(fileAddressArray[i]),
                    RequestPath = "/Daemon",
                    ContentTypeProvider = provider
                });
            }
        }
    }
}
