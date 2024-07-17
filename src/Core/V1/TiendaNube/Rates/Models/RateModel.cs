using System;
using System.Collections.Generic;
using System.Text;

namespace Core.V1.TiendaNube.Rates.Models
{
    public class RateModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public double Price { get; set; }
        public double Price_merchant { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public DateTime Min_delivery_date { get; set; }
        public DateTime Max_delivery_date { get; set; }
        public bool Phone_required { get; set; }
        public string Reference { get; set; }
        public bool? Id_required { get; set; }
        public bool? Accepts_cod { get; set; }
        public AddressModel Address { get; set; }
        public List<HourModel> Hours { get; set; }
        public bool? Availability { get; set; }
    }
}
