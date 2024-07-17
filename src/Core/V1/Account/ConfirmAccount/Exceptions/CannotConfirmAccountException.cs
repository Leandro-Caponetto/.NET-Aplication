using Core.Exceptions;

namespace Core.V1.Account.ConfirmAccount.Exceptions
{
    public class CannotConfirmAccountException : BusinessException
    {
        public CannotConfirmAccountException()
            : base("Ha ocurrido un error al confirmar la cuenta.")
        {

        }
    }
}
