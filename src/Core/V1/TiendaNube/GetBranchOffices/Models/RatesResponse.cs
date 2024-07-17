using System;
using System.Collections.Generic;

namespace Core.V1.TiendaNube.GetBranchOffices.Models
{
    
    public class RatesResponse
    {
        public RatesResponse()
        {
            this.hours = new List<HourModel>();
        }

        public string name { get; set; }
        public string code { get; set; }
        public double price { get; set; }
        public double price_merchant { get; set; }
        public string currency { get; set; }
        public string type { get; set; }
        //public string min_delivery_date { get; set; }
        //public string max_delivery_date { get; set; }

        public DateTimeOffset min_delivery_date { get; set; }
        public DateTimeOffset max_delivery_date { get; set; }

        public bool phone_required { get; set; }
        public bool id_required { get; set; }
        public bool accepts_cod { get; set; }
        public bool availability { get; set; }
        public string reference { get; set; }
        public AddressModel address { get; set; }
        public IEnumerable<HourModel> hours { get; set; }

    }
}
