using Core.Data.Repositories;
using Core.Shared.Services;
using Core.V1.Account.CanExtendSession.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.Account.CanExtendSession
{
    public class CanExtendSessionRequestHandler : IRequestHandler<CanExtendSessionRequest, CanExtendSessionResult>
    {
        private readonly ISessionRepository sessionRepository;
        private readonly IDateTimeOffsetService dateTimeOffsetService;

        public CanExtendSessionRequestHandler(ISessionRepository sessionRepository, IDateTimeOffsetService dateTimeOffsetService)
        {
            this.sessionRepository = sessionRepository ?? throw new System.ArgumentNullException(nameof(sessionRepository));
            this.dateTimeOffsetService = dateTimeOffsetService ?? throw new ArgumentNullException(nameof(dateTimeOffsetService));
        }

        public async Task<CanExtendSessionResult> Handle(CanExtendSessionRequest request, CancellationToken cancellationToken)
        {
            var session = await sessionRepository.FindByIdAsync(request.RefreshToken);

            return new CanExtendSessionResult(
                isValid: session != null && session.Active && session.ExpiresOn > dateTimeOffsetService.UtcNow());
        }
    }
}
