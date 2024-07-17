using FluentValidation;

namespace Core.V1.TiendaNube.ShippingCarrier
{
    public class ShippingCarrierRequestValidator : AbstractValidator<ShippingCarrierRequest>
    {
        public ShippingCarrierRequestValidator()
        {
            RuleFor(m => m.Code)
                .NotEmpty();

        }
    }
}
