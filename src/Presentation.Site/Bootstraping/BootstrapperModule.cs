using Autofac;
using Microsoft.Extensions.Configuration;
using System;

namespace Presentation.Site.Bootstraping
{
    public class BootstrapperModule : Module
    {
        private readonly IConfiguration configuration;

        public BootstrapperModule(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new MediatRModule());
            builder.RegisterModule(new CoreModule(configuration));
            builder.RegisterModule(new DataModule());
            builder.RegisterModule(new HTTPRequestServicesModule(configuration));
            builder.RegisterModule(new CacheModule(configuration));

            base.Load(builder);
        }
    }
}
