using MediatR;

namespace Core.V1.Account.Logout
{
    public class LogoutRequest : IRequest
    {
        public LogoutRequest(string userId, string accessToken)
        {
            UserId = userId;
            AccessToken = accessToken;
        }

        public string UserId { get; set; }

        public string AccessToken { get; set; }
    }
}
