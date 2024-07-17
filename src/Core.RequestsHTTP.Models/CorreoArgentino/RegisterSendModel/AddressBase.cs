using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.ApiMiCorreo.RegisterSendModel
{
    public class AddressBase
    {

        public AddressBase()
        {

        }
        public string streetName { get; set; }
        public string streetNumber { get; set; }
        public string floor { get; set; }
        public string apartment { get; set; }
        public string locality { get; set; }
        public string city { get; set; }
        public string provinceCode { get; set; }
        public string postalCode { get; set; }
    }
}
