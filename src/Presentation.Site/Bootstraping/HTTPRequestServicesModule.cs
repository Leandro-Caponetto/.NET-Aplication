using Autofac;
using Core.Data.RequestServices;
using Core.RequestsHTTP;
using Core.RequestsHTTP.RequestServices;
using Core.V1;
using Microsoft.Extensions.Configuration;
using Serilog;


namespace Presentation.Site.Bootstraping
{
    public class HTTPRequestServicesModule : Autofac.Module
    {
        private readonly IConfiguration configuration;

        public HTTPRequestServicesModule(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder
                .RegisterType<HttpClientSender>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder
                .Register(c => new TiendaNubeRequestService(
                    c.Resolve<IHttpClientSender>(),
                    configuration["TiendaNube:UrlService"],
                    configuration["TiendaNube:Client_id"],
                    configuration["TiendaNube:Client_secret"],
                    configuration["TiendaNube:Grant_type"],
                    configuration["TiendaNube:User_agent_request"],
                    configuration["TiendaNube:ApiUrlService"],
                    configuration["TiendaNubeShipping:name"],
                    configuration["TiendaNubeShipping:callback_url"],
                    configuration["TiendaNubeShipping:types"]
                    ))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder
                .Register(c => new ApiMiCorreoServices(
                    c.Resolve<IHttpClientSender>(),
                    configuration["CorreoArg:UrlService"],
                    configuration["CorreoArg:Username"],
                    configuration["CorreoArg:Password"]))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder
                .Register(c => new Agencies(
                    c.Resolve<IApiMiCorreo>(),
                    c.Resolve<ILogger>()))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}
