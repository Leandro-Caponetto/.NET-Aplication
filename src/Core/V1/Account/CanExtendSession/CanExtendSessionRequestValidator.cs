using FluentValidation;

namespace Core.V1.Account.CanExtendSession
{
    public class CanExtendSessionRequestValidator : AbstractValidator<CanExtendSessionRequest>
    {
        public CanExtendSessionRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .NotNull();
        }
    }
}
