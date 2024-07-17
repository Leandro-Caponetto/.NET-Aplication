using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.TiendaNube
{
    public class StoreReponseModel
    {
        public string email { get; set; }
        public string contact_email { get; set; }
        public string country { get; set; }
        public string business_id  { get; set; }
        public string business_name { get; set; }
        public string business_address { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string original_domain { get; set; }
    }
}
