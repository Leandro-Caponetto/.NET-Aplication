using FluentValidation;

namespace Core.V1.TiendaNube.RegisterTienda
{
    public class RegisterTiendaRequestValidator : AbstractValidator<RegisterTiendaRequest>
    {
        public RegisterTiendaRequestValidator()
        {
            RuleFor(m => m.Code)
                .NotEmpty();

        }
    }
}
