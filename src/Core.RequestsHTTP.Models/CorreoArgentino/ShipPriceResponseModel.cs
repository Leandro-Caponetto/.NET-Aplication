using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.ApiMiCorreo
{
    public class Rates
    {
        public string deliveredType { get; set; }
        public string productType { get; set; }
        public string productName { get; set; }
        public float price { get; set; }
        public string deliveryTimeMin { get; set; }
        public string deliveryTimeMax { get; set; }
    }

    public class ShipPriceResponseModel
    {
        public string customerId { get; set; }
        public string validTo { get; set; }
        public List<Rates> rates { get; set; }
        public string code { get; set; }
        public string message { get; set; }
    }

}