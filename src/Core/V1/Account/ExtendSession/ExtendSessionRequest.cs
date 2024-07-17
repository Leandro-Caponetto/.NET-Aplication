using MediatR;
using System;

namespace Core.V1.Account.ExtendSession
{
    public class ExtendSessionRequest : IRequest
    {
        public ExtendSessionRequest(string userId, string oldRefreshToken, /*string newRefreshToken,*/ string accessToken, TimeSpan validFor)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            OldRefreshToken = oldRefreshToken ?? throw new ArgumentNullException(nameof(oldRefreshToken));
            //NewRefreshToken = newRefreshToken ?? throw new ArgumentNullException(nameof(newRefreshToken));
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            ValidFor = validFor;
        }

        public string UserId { get; }
        public string OldRefreshToken { get; }
        //public string NewRefreshToken { get; }
        public string AccessToken { get; }
        public TimeSpan ValidFor { get; }
    }
}
