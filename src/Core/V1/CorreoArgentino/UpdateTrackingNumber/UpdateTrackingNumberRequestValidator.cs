using FluentValidation;

namespace Core.V1.CorreoArgentino.UpdateTrackingNumber
{
    public class UpdateTrackingNumberRequestValidator : AbstractValidator<UpdateTrackingNumberRequest>
    {
        public UpdateTrackingNumberRequestValidator()
        {
            RuleFor(m => m.date)
                .NotEmpty();

            RuleFor(m => m.customerId)
                .NotEmpty();

            RuleFor(m => m.extOrderId)
                .NotEmpty();

            RuleFor(m => m.trackingNumber)
                .NotEmpty();

        }
    }
}
