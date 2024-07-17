using System;

namespace Core.Shared.Configuration
{
    public class FrontendOptions
    {
        public FrontendOptions()
        {

        }

        public FrontendOptions(string url, string confirmAccount, string forgotPassword)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            ConfirmAccount = confirmAccount ?? throw new ArgumentNullException(nameof(confirmAccount));
            ForgotPassword = forgotPassword ?? throw new ArgumentNullException(nameof(forgotPassword));
        }

        public string Url { get; }

        public string ConfirmAccount { get; }

        public string ForgotPassword { get; }
    }
}
