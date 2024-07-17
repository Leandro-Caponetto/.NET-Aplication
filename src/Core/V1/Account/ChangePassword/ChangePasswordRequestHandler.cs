using Core.Entities;
using Core.V1.Account.ChangePassword.Exceptions;
using Core.V1.Account.ConfirmAccount.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.Account.ChangePassword
{
    public class ChangePasswordRequestHandler : IRequestHandler<ChangePasswordRequest>
    {
        private readonly UserManager<User> userManager;
        private readonly ILogger logger;

        public ChangePasswordRequestHandler(UserManager<User> userManager, ILogger logger)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information("Start service: api/Account/changepassword...");

            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                te.Stop();
                logger.Error("End service api/Account/changepassword 1- elapsed time {0}ms", te.ElapsedMilliseconds);
                throw new NotExistentUserException();
            }

            var isSamePassword = await userManager.CheckPasswordAsync(user, request.Password);

            if (isSamePassword)
            {
                te.Stop();
                logger.Error("End service api/Account/changepassword 2- elapsed time {0}ms", te.ElapsedMilliseconds);
                throw new DoNotMeetPreestablishedParametersException();
            }

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.Password);

            if (!result.Succeeded)
            {
                te.Stop();
                logger.Error("End service api/Account/changepassword 3- elapsed time {0}ms", te.ElapsedMilliseconds);
                throw new CannotResetPasswordException();
            }

            te.Stop();
            logger.Information("End service api/Account/changepassword elapsed time {0}ms", te.ElapsedMilliseconds);
            return Unit.Value;
        }
    }
}
