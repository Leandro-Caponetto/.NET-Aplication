using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.ApiMiCorreo
{
    public class GetAgencyModel
    {
        public string customerGroup { get; set; }
        public string customerId { get; set; }
        public string services { get; set; }
        public char provinceCode { get; set; }

    }

}