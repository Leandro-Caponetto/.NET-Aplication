using Core.Exceptions;

namespace Core.V1.Account.ChangePassword.Exceptions
{
    public class CannotResetPasswordException : BusinessException
    {
        public CannotResetPasswordException()
            : base("Ocurrió un error al recuperar la contraseña. Intente de nuevo más tarde.")
        {
        }
    }
}
