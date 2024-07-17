using System;
using System.Collections.Immutable;
using System.Security.Claims;

namespace Core.RequestsHTTP.Models.ApiMiCorreo
{
    public class UpdateOrderModel
    {
        public UpdateOrderModel()
        {

        }

        public string shipping_tracking_number { get; set; }
        public string shipping_tracking_url { get; set; }
        public bool notify_customer { get; set; }
  
    }
}
