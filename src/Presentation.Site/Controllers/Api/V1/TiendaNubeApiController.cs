using System.Collections.Generic;
using System.Threading.Tasks;
using Core.V1.TiendaNube.GetBranchOffices;
using Core.V1.TiendaNube.GetBranchOffices.Models;
using Core.V1.TiendaNube.ResetCache;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Presentation.Site.Auth.Jwt;


namespace Presentation.Site.Controllers.Api.V1
{
    [Route("api/[controller]")]
    //[ApiVersion("1.0")]
    [ApiController]
    public class TiendaNubeApiController : BaseController
    {
        private readonly IMediator mediator;

        public TiendaNubeApiController(IMediator mediator, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions)
        {
            this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [AllowAnonymous]
        [HttpPost("GetBrachOffices")]
        public async Task<RatesModelResponse> GetBrachOffices([FromBody]GetBranchOfficesRequest request)
        {
            var response = await mediator.Send(request);

            return response;
        }

    }
}
