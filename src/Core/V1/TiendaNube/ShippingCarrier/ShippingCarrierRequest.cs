using MediatR;

namespace Core.V1.TiendaNube.ShippingCarrier
{
    public class ShippingCarrierRequest : IRequest
    {
        public string Code { get; set; }
    }
}
