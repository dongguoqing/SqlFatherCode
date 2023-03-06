using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blueearth
{
    public class MyAutofacModule:Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //Autofac 基于配置文件的服务注册
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("autofac.json");
            IConfigurationRoot root = configurationBuilder.Build();
            //开始读取配置文件中的内容
            ConfigurationModule module = new ConfigurationModule(root);
            //根据配置文件的内容注册服务
            builder.RegisterModule(module);
        }
    }
}
