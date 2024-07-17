using Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Core.V1.TiendaNube.RegisterTienda.Exceptions
{
    public class CannotRegisterAccountException : BusinessException
    {
        public CannotRegisterAccountException(IEnumerable<IdentityError> errors)
            : base("No se puede registrar el usuario.")
        {
        }
    }
}
