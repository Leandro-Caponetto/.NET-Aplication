using MediatR;

namespace Core.V1.Account.ChangePassword
{
    public class ChangePasswordRequest : IRequest
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordRepeat { get; set; }
    }
}
