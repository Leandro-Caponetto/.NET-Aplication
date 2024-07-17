using Core.Shared;
using MediatR;

namespace Core.V1.Account.ConfirmAccount
{
    public class ConfirmAccountRequest : LoggedRequest, IRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
