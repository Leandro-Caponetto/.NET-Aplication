using Core.Entities;
using Core.V1.Account.Register.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.Account.Register
{
    public class RegisterRequestHandler : IRequestHandler<RegisterRequest>
    {
        private readonly UserManager<User> userManager;
        private readonly ILogger logger;

        public RegisterRequestHandler(UserManager<User> userManager, ILogger logger)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(RegisterRequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information("Start service: api/Account/register...");

            var user = new User(request.Email) 
            { 
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedOn = DateTime.Now,
                EmailConfirmed = true
            };

           var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                te.Stop();
                logger.Error("End service api/Account/register elapsed time {0}ms", te.ElapsedMilliseconds);
                throw new CannotRegisterAccountException(result.Errors);
            }

            te.Stop();
            logger.Information("End service api/Account/register elapsed time {0}ms", te.ElapsedMilliseconds);
            return Unit.Value;
        }
    }
}
