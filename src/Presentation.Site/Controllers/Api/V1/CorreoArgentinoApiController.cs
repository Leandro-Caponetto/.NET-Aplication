using System.Threading.Tasks;
using Core.V1.CorreoArgentino.UpdateTrackingNumber;
using Core.V1.CorreoArgentino.UpdateTrackingNumber.Models;
using Core.V1;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Presentation.Site.Auth.Jwt;


namespace Presentation.Site.Controllers.Api.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class CorreoArgentinoApiController : BaseController
    {
        private readonly IMediator mediator;

        public CorreoArgentinoApiController(IMediator mediator, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions)
        {
            this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Authorize(Roles = "CAAdministrator")]
        [HttpPost("UpdateOrderTrackingNumber")]
        public async Task<ErrorTrackingNumberModel> UpdateOrderTrackingNumber([FromBody]UpdateTrackingNumberRequest request)
        {
            return await mediator.Send(request);
        }

        [AllowAnonymous]
        [HttpGet("version")]
        public Version Version()
        {
            return new Version();
        }

        [AllowAnonymous]
        [HttpGet("monitor")]
        public async Task<Monitor> Monitor()
        {
            MonitorRequest request = new MonitorRequest();
            return await mediator.Send(request);
        }
    }
}