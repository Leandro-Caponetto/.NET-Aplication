using Core.Shared;
using Core.V1.Account.Get.Models;
using MediatR;

namespace Core.V1.Account.Get
{
    public class GetAccountRequest : LoggedRequest, IRequest<AccountModel>
    {
    }
}
