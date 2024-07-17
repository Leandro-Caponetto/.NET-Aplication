using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.TiendaNube
{
    public class ShippingCarrierModel
    {
        public string name { get; set; }
        public string callback_url { get; set; }
        public string types { get; set; }
    }
}
