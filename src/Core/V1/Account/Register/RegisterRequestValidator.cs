using FluentValidation;

namespace Core.V1.Account.Register
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(m => m.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(m => m.FirstName)
                .NotEmpty();

            RuleFor(m => m.LastName)
                .NotEmpty();

            RuleFor(m => m.Password)
                .NotEmpty()
                .Equal(x => x.PasswordRepeat);

            RuleFor(m => m.PasswordRepeat)
                .NotEmpty()
                .Equal(x => x.Password);
        }
    }
}
