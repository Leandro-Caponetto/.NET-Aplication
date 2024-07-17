using Core.V1.Account.CanExtendSession.Models;
using MediatR;

namespace Core.V1.Account.CanExtendSession
{
    public class CanExtendSessionRequest : IRequest<CanExtendSessionResult>
    {
        public string RefreshToken { get; set; }
    }
}
