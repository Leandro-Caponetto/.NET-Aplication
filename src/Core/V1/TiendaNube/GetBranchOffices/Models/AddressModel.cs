using System;
using System.Collections.Generic;
using System.Text;

namespace Core.V1.TiendaNube.GetBranchOffices.Models
{
    public class AddressModel
    {
        public string address { get; set; }
        public string number { get; set; }
        public string floor { get; set; }
        public string locality { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string phone { get; set; }
        public string zipcode { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }

}
