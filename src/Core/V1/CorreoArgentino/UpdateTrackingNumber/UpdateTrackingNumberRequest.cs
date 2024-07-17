using Core.V1.CorreoArgentino.UpdateTrackingNumber.Models;
using MediatR;

namespace Core.V1.CorreoArgentino.UpdateTrackingNumber
{
    public class UpdateTrackingNumberRequest : IRequest<ErrorTrackingNumberModel>
    {
        public string customerId { get; set; }
        public string extOrderId { get; set; }
        public string trackingNumber { get; set; }
        public string date { get; set; }

    }
}