using System;
using System.Collections.Generic;
using System.Text;

namespace Core.V1.TiendaNube.Rates.Models
{
    public class AddressModel
    {
        public string Address { get; set; }
        public string Number { get; set; }
        public object Floor { get; set; }
        public string Locality { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Zipcode { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
