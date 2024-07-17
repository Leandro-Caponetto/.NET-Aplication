using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.ApiMiCorreo.RegisterSendModel
{
    public class Shipping
    {
        public Shipping()
        {

        }

        public string productType { get; set; }
        public string deliveryType { get; set; }
        public string agency { get; set; }

        public AddressBase address { get; set; }

        public int weight { get; set; }
        public float declaredValue { get; set; }
        public int height { get; set; }
        public int length { get; set; }
        public int width { get; set; }
    }

}
