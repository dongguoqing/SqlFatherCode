using Autofac;
using Daemon.Infrustructure.Contract;
using Daemon.Infrustructure.EF;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Daemon.Repository.EF
{
	public class RepositoryModule : Autofac.Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			//RegisterAssemblyTypes 扫描程序集
			//AsImplementedInterfaces 表示接口的注册 但不实现Idispose接口
			//InstancePerLifetimeScope 一个生命周期域中，每一个依赖或调用创建一个单一的共享的实例，且每一个不同的生命周期域，实例是唯一的，不共享的
			builder.RegisterType<EFUnitOfWork>()
				.As<IUnitOfWork>()
				.InstancePerLifetimeScope();
			builder.RegisterAssemblyTypes(this.ThisAssembly)
				.Where(t => t.IsAssignableTo<IRepository>())
				.AsImplementedInterfaces()
				.InstancePerLifetimeScope();
		}
	}
}
