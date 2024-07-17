using Core.Data.Repositories;
using Core.Data.RequestServices;
using Core.RequestsHTTP;
using MediatR;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.Account.Logout
{
    public class LogoutRequestHandler : IRequestHandler<LogoutRequest>
    {
        private readonly ITiendaNubeRequestService tiendaNubeRequestService;
        private readonly ISessionRepository sessionRepository;
        private readonly ILogger logger;

        public LogoutRequestHandler(ITiendaNubeRequestService tiendaNubeRequestService, ISessionRepository sessionRepository, ILogger logger)
        {
            this.tiendaNubeRequestService = tiendaNubeRequestService ?? throw new ArgumentNullException(nameof(tiendaNubeRequestService));
            this.sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(LogoutRequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information("Start service: api/Account/logout...");

            var session = await sessionRepository.FindByUserIdAndToken(request.UserId, request.AccessToken);

            sessionRepository.Delete(session);

            te.Stop();
            logger.Information("End service api/Account/logout elapsed time {0}ms", te.ElapsedMilliseconds);
            return Unit.Value;
        }
    }
}
