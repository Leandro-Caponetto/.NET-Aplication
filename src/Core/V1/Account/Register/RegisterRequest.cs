using MediatR;

namespace Core.V1.Account.Register
{
    public class RegisterRequest : IRequest
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string PasswordRepeat { get; set; }
    }
}
