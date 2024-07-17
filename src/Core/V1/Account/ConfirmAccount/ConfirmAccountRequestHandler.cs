using Core.Entities;
using Core.V1.Account.ConfirmAccount.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.Account.ConfirmAccount
{
    public class ConfirmAccountRequestHandler : IRequestHandler<ConfirmAccountRequest>
    {
        private readonly UserManager<User> userManager;
        private readonly ILogger logger;

        public ConfirmAccountRequestHandler(UserManager<User> userManager, ILogger logger)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(ConfirmAccountRequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information("Start service: api/Account/confirm...");

            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                throw new NotExistentUserException();
            }

            user.EmailConfirmed = true;

            var result = await userManager.ConfirmEmailAsync(user, request.Token);

            if (!result.Succeeded)
            {
                te.Stop();
                logger.Error("End service api/Account/confirm elapsed time {0}ms", te.ElapsedMilliseconds);
                throw new CannotConfirmAccountException();
            }

            te.Stop();
            logger.Information("End service api/Account/confirm elapsed time {0}ms", te.ElapsedMilliseconds);
            return Unit.Value;
        }
    }
}
