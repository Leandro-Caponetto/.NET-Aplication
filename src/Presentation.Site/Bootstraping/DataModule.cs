using Autofac;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Reflection;

namespace Presentation.Site.Bootstraping
{
    public class DataModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            RegisterRepositories(builder);
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            var assemblies = DependencyContext.Default
                .GetDefaultAssemblyNames()
                .Select(x => Assembly.Load(x));

            // request handlers
            foreach (var assembly in assemblies)
            {
                RegisterRepositories(builder, assembly);
            }
        }

        private void RegisterRepositories(ContainerBuilder builder, Assembly assembly)
        {
            builder
                .RegisterAssemblyTypes(assembly)
                .Where(t => t.IsClass && t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}
