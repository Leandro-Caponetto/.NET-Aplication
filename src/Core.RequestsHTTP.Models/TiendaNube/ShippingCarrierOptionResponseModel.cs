using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.TiendaNube
{
    public class ShippingCarrierOptionResponseModel
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public int additional_days { get; set; }
        public double additional_cost { get; set; }
        public bool allow_free_shipping { get; set; }
        public bool active { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
