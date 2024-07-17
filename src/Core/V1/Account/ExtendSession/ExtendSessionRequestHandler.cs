using Core.Data.Repositories;
using Core.Shared.Services;
using MediatR;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.Account.ExtendSession
{
    public class ExtendSessionRequestHandler : IRequestHandler<ExtendSessionRequest>
    {
        private readonly ISessionRepository sessionRepository;
        private readonly IDateTimeOffsetService dateTimeOffsetService;
        private readonly ILogger logger;

        public ExtendSessionRequestHandler(ISessionRepository sessionRepository, IDateTimeOffsetService dateTimeOffsetService, ILogger logger)
        {
            this.sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
            this.dateTimeOffsetService = dateTimeOffsetService ?? throw new ArgumentNullException(nameof(dateTimeOffsetService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(ExtendSessionRequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information("Start service: api/Account/refresh...");

            var session = await sessionRepository.FindByIdAsync(request.OldRefreshToken);

            session.Token = request.AccessToken;
            session.ExpiresOn = dateTimeOffsetService.UtcNow().Add(request.ValidFor);

            sessionRepository.Update(session);

            te.Stop();
            logger.Information("End service api/Account/refresh elapsed time {0}ms", te.ElapsedMilliseconds);
            return Unit.Value;
        }
    }
}
