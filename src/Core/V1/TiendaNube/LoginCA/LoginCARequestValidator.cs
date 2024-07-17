using FluentValidation;

namespace Core.V1.TiendaNube.LoginCA
{
    public class LoginCARequestValidator : AbstractValidator<LoginCARequest>
    {
        public LoginCARequestValidator()
        {
            RuleFor(m => m.Username)
                .NotEmpty();

            RuleFor(m => m.Password)
                .NotEmpty();

        }
    }
}
