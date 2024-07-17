using Core.Data.Repositories;
using Core.Entities;
using Core.Shared.Services;
using MediatR;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.Account.InitSession
{
    public class InitSessionRequestHandler : IRequestHandler<InitSessionRequest>
    {
        private readonly ISessionRepository sessionRepository;
        private readonly IDateTimeOffsetService dateTimeOffsetService;
        private readonly ILogger logger;

        public InitSessionRequestHandler(
            ISessionRepository sessionRepository,
            IDateTimeOffsetService dateTimeOffsetService,
            ILogger logger)
        {
            this.sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
            this.dateTimeOffsetService = dateTimeOffsetService ?? throw new ArgumentNullException(nameof(dateTimeOffsetService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Unit> Handle(InitSessionRequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information("Start service: api/Account/login...");

            var session = new Session(
                request.RefreshToken,
                request.UserId,
                request.Token,
                expiresOn: dateTimeOffsetService.UtcNow().Add(request.ValidFor),
                createdOn: dateTimeOffsetService.UtcNow());

            sessionRepository.Add(session);

            te.Stop();
            logger.Information("End service api/Account/login elapsed time {0}ms", te.ElapsedMilliseconds);
            return Unit.Task;
        }
    }
}
