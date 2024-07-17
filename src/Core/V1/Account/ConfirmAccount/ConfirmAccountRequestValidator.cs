using FluentValidation;

namespace Core.V1.Account.ConfirmAccount
{
    public class ConfirmAccountRequestValidator : AbstractValidator<ConfirmAccountRequest>
    {
        public ConfirmAccountRequestValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .NotEmpty();

            RuleFor(x => x.Token)
                .NotEmpty();
        }
    }
}
