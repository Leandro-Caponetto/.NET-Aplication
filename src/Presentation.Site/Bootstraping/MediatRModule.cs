using Autofac;
using Core.Data.EF.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Reflection;

namespace Presentation.Site.Bootstraping
{
    public class MediatRModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            RegisterMediatR(builder);
            RegisterHandlers(builder);
            RegisterTransactionalBehavior(builder);
        }

        private void RegisterMediatR(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
        }

        private void RegisterHandlers(ContainerBuilder builder)
        {
            var assemblies = DependencyContext.Default
                .GetDefaultAssemblyNames()
                .Select(x => Assembly.Load(x));

            // request handlers
            foreach (var assembly in assemblies)
            {
                RegisterHandlers(builder, assembly);
            }
        }

        private void RegisterHandlers(ContainerBuilder builder, Assembly assembly)
        {
            builder
                .RegisterAssemblyTypes(assembly)
                .Where(t => t.GetInterfaces().Where(i => i.IsClosedTypeOf(typeof(IRequestHandler<>))).Any())
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder
                .RegisterAssemblyTypes(assembly)
                .Where(t => t.GetInterfaces().Where(i => i.IsClosedTypeOf(typeof(IRequestHandler<,>))).Any())
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder
                .RegisterAssemblyTypes(assembly)
                .Where(t => t.GetInterfaces().Where(i => i.IsClosedTypeOf(typeof(INotificationHandler<>))).Any())
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }

        private static void RegisterTransactionalBehavior(ContainerBuilder builder)
        {
            builder
                .RegisterGeneric(typeof(TransactionalBehavior<,>))
                .As(typeof(IPipelineBehavior<,>))
                .InstancePerLifetimeScope();
        }
    }
}
