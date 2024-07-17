using MediatR;
using System;

namespace Core.V1.Account.InitSession
{
    public class InitSessionRequest : IRequest
    {
        public InitSessionRequest(string userId, string token, string refreshToken, TimeSpan validFor)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            Token = token ?? throw new ArgumentNullException(nameof(token));
            RefreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
            ValidFor = validFor;
        }

        public string UserId { get; }

        public string Token { get; set; }

        public string RefreshToken { get; }

        public TimeSpan ValidFor { get; }
    }
}
