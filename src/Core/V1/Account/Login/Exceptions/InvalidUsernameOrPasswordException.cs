using Core.Exceptions;

namespace Core.V1.Account.Login.Exceptions
{
    public class InvalidUsernameOrPasswordException : BusinessException
    {
        public InvalidUsernameOrPasswordException()
            : base("El usuario o la contraseña son invalidos")
        {
        }
    }
}
