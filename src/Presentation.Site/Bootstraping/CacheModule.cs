using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Linq;
using System.Reflection;

namespace Presentation.Site.Bootstraping
{
    public class CacheModule : Autofac.Module
    {
        private readonly TimeSpan expirationCacheTime;
        private readonly IConfiguration configuration;

        public CacheModule(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
            expirationCacheTime = TimeSpan.FromHours(Convert.ToInt32(this.configuration["CacheExpirationTime"]));
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            RegisterCacheServices(builder);
        }

        private void RegisterCacheServices(ContainerBuilder builder)
        {
            var assemblies = DependencyContext.Default
                .GetDefaultAssemblyNames()
                .Select(x => Assembly.Load(x));

            // request handlers
            foreach (var assembly in assemblies)
            {
                RegisterCacheServices(builder, assembly);
            }
        }

        private void RegisterCacheServices(ContainerBuilder builder, Assembly assembly)
        {
            builder
                .RegisterAssemblyTypes(assembly)
                .Where(t => t.IsClass && t.Name.EndsWith("CacheService"))
                .WithParameter("expirationTime", expirationCacheTime)
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
