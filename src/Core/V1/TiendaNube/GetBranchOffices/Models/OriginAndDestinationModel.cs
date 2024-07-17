using System;
using System.Collections.Generic;
using System.Text;

namespace Core.V1.TiendaNube.GetBranchOffices.Models
{
    
    public class OriginAndDestinationModel
    {
        public string name { get; set; }
        public string address { get; set; }
        public string number { get; set; }
        public string floor { get; set; }
        public string locality { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string postal_code { get; set; }
        public string phone { get; set; }
    }

}
