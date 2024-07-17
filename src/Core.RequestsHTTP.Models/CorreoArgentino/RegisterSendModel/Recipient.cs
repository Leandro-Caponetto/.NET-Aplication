using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.ApiMiCorreo.RegisterSendModel
{
    public class Recipient
    {
        public Recipient()
        {

        }
        public string name { get; set; }
        public string phone { get; set; }
        public string cellPhone { get; set; }
        public string email { get; set; }

    }


}
