using Core.Exceptions;

namespace Core.V1.Account.ConfirmAccount.Exceptions
{
    public class NotExistentUserException : BusinessException
    {
        public NotExistentUserException()
            : base("Cuenta inválida.")
        {

        }
    }
}
