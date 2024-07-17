using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.TiendaNube
{
    public class ShippingCarrierResponseModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public string callback_url { get; set; }
        public string types { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}

