using MediatR;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Core.V1.Account.Get;
using Presentation.Site.Helpers.Extensions;

namespace Presentation.Site.Auth
{
    public class ConfirmAccountRequirementHandler : AuthorizationHandler<ConfirmAccountRequirement>
    {
        private readonly IMediator mediator;

        public ConfirmAccountRequirementHandler(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ConfirmAccountRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                return;
            }

            var request = new GetAccountRequest();
            request.SetUserName(context.User.UserName());
            var response = await mediator.Send(request);

            if (response.EmailConfirmed)
            {
                context.Succeed(requirement);
            }
        }
    }
}
