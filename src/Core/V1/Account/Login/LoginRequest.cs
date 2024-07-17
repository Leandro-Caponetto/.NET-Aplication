using Core.V1.Account.Login.Models;
using MediatR;

namespace Core.V1.Account.Login
{
    public class LoginRequest : IRequest<LoginModel>
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
