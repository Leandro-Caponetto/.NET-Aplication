using System.Linq;
using System.Threading.Tasks;
using Core.V1.Account.ConfirmAccount;
using Core.V1.Account.Login;
using Core.V1.Account.Register;
using Core.V1.Account.InitSession;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Presentation.Site.Auth.Jwt;
using Presentation.Site.Helpers.Models;
using Core.V1.Account.CanExtendSession;
using Presentation.Site.Helpers.Extensions;
using Core.V1.Account.ExtendSession;
using Core.V1.Account.Logout;
using Core.V1.Account.ChangePassword;

namespace Presentation.Site.Controllers.Api.V1
{
    [Route("api/[controller]")]
    //[ApiVersion("1.0")]
    //[Route("v{version:apiVersion}/account")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IMediator mediator;
        private readonly IJwtFactory jwtFactory;
        private readonly IOptions<JwtIssuerOptions> jwtOptions;

        public AccountController(IMediator mediator, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions)
        {
            this.mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            this.jwtFactory = jwtFactory ?? throw new System.ArgumentNullException(nameof(jwtFactory));
            this.jwtOptions = jwtOptions ?? throw new System.ArgumentNullException(nameof(jwtOptions));
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            await mediator.Send(request);

            return OkNoContent();
        }

        [AllowAnonymous]
        [HttpPost("confirm")]
        public async Task<ActionResult> Confirm(ConfirmAccountRequest request)
        {
            await mediator.Send(request);

            return OkNoContent();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResult>> Login(LoginRequest request)
        {
            // Verify that the user & pass are on db and match
            var result = await mediator.Send(request);

            // Create an identity for the user
            var identity = jwtFactory.GenerateClaimsIdentity(request.Email, result.Id, result.Claims);

            // With the identity generate a jwt and a refresh token
            var accessToken = await jwtFactory.GenerateEncodedTokenAsync(request.Email, identity);
            var refreshToken = jwtFactory.GenerateRefreshToken();

            // Save tokens into db
            await mediator.Send(
                new InitSessionRequest(result.Id, accessToken, refreshToken, jwtOptions.Value.RefreshTokenValidFor));

            // Reply with both tokens
            return Ok(new LoginResult(accessToken, refreshToken));
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResult>> Refresh(CanExtendSessionRequest request)
        {
            // Verify that the refreshToken is not expired and active
            var result = await mediator.Send(request);

            if (!result.IsValid)
            {
                return Unauthorized();
            }

            // Get the identity that is within the jwt
            var identity = jwtFactory.GetPrincipalFromToken(
                token: Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""), allowExpired: true);

            // With the identity generate a new jwt and refresh token
            var accessToken = await jwtFactory.GenerateEncodedTokenAsync(identity.UserName(), identity.Identities.First());
            //var refreshToken = jwtFactory.GenerateRefreshToken();

            // Extend the session => deleting old session and creating a new one
            await mediator.Send(
                new ExtendSessionRequest(identity.Id(), request.RefreshToken, /*refreshToken,*/ accessToken, jwtOptions.Value.RefreshTokenValidFor));

            // Reply with both new tokens
            return Ok(new LoginResult(accessToken, request.RefreshToken));
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            // Get jwt from request
            var token = Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");

            // Delete session with userId and jwt
            await mediator.Send(new LogoutRequest(User.Id(), token));

            // Reply Ok
            return OkNoContent();
        }

        [AllowAnonymous]
        [HttpPost("changepassword")]
        public async Task<ActionResult> ChangePassword(ChangePasswordRequest request)
        {
            await mediator.Send(request);

            return OkNoContent();
        }
    }
}
