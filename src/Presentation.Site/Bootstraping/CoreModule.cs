using Autofac;
using Core.Shared.Configuration;
using Core.Shared.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Autofac.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Presentation.Site.Bootstraping
{
    public class CoreModule : Autofac.Module
    {
        private readonly IConfiguration configuration;

        public CoreModule(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder
                .RegisterInstance(configuration
                    .GetSection(nameof(FrontendOptions))
                    .Get<FrontendOptions>(x => x.BindNonPublicProperties = true))
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MinMaxDeliveryDays>()
                .WithParameter("min", int.Parse(configuration["CorreoArg:MinDeliveryDays"]))
                .WithParameter("max", int.Parse(configuration["CorreoArg:MaxDeliveryDays"]))
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<TrackingSettingsCorreoArgentino>()
                .WithParameter("url", configuration["CorreoArg:ShippingUrl"])
                .AsSelf()
                .SingleInstance();

            builder
                .Register(FrontendOptionsFactory)
                .AsSelf()
                .SingleInstance();


            RegisterDateTimeOffsetService(builder);

            RegisterValidators(builder, Assembly.Load("Core"));

            RegisterSerilogLogger(builder);

        }

        private FrontendOptions FrontendOptionsFactory(IComponentContext ctx)
        {
            var config = ctx.Resolve<IConfiguration>();
            return new FrontendOptions(
                config["FrontendOptions:Url"],
                config["FrontendOptions:ConfirmAccount"],
                config["FrontendOptions:ForgotPassword"]);
        }

        private void RegisterDateTimeOffsetService(ContainerBuilder builder)
        {
            builder
                .RegisterType<DateTimeOffsetService>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        private void RegisterValidators(ContainerBuilder builder, Assembly assembly)
        {
            builder
                .RegisterAssemblyTypes(assembly)
                .Where(t => t.IsClass && t.Name.EndsWith("Validator"))
                .AsSelf()
                .InstancePerLifetimeScope();
        }

        private void RegisterSerilogLogger(ContainerBuilder builder)
        {
            //var loggerConfig = new LoggerConfiguration()
            //    .Enrich.WithMachineName()
            //    .Enrich.WithEnvironmentUserName()
            //    .WriteTo.File(
            //        configuration["Logging:LogPath"],
            //        rollingInterval: Enum.Parse<RollingInterval>(configuration["Logging:LogRollingInterval"]),
            //        restrictedToMinimumLevel: Enum.Parse<LogEventLevel>(configuration["Logging:LogLevel"]),
            //        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [MachineName: {MachineName}] [EnviromentUserName: {EnvironmentUserName}] {Message}{NewLine}{Exception}"
            //    );

            //builder.RegisterSerilog(loggerConfig);

            builder
                .Register(service => new LoggerConfiguration()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentUserName()
                    .Enrich.With(new ThreadIDEnricher())
                    .WriteTo.File(
                        configuration["Logging:LogPath"],
                        restrictedToMinimumLevel: Enum.Parse<LogEventLevel>(configuration["Logging:LogLevel"]),
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}:{Level:u3}:{ThreadID}-{Message}{NewLine}{Exception}",
                        fileSizeLimitBytes: 1024 * 1024 * 1024,
                        rollingInterval: Enum.Parse<RollingInterval>(configuration["Logging:LogRollingInterval"]),
                        rollOnFileSizeLimit: true
                    ).CreateLogger()
                )
                .As<ILogger>()
                .SingleInstance();
        }
    }
}
