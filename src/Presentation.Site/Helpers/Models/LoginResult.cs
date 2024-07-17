using System;

namespace Presentation.Site.Helpers.Models
{
    public class LoginResult
    {
        public LoginResult(string accessToken, string refreshToken)
        {
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            RefreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
        }

        public string AccessToken { get; }

        public string RefreshToken { get; }
    }
}
