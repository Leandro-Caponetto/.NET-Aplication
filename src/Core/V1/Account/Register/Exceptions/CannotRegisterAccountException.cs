using Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Core.V1.Account.Register.Exceptions
{
    public class CannotRegisterAccountException : BusinessException
    {
        public CannotRegisterAccountException(IEnumerable<IdentityError> errors)
            : base("No se puede registrar el usuario.")
        {
        }
    }
}
