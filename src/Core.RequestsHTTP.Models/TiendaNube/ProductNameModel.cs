using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.TiendaNube
{
    public class ProductNameModel
    {
        public string id { get; set; }
        public Name name { get; set; }
    }

    public class Name
    {
        public string en { get; set; }
        public string es { get; set;}
        public string pt { get; set;}
    }
}
