using Core.RequestsHTTP.Models.ApiMiCorreo.RegisterSendModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.ApiMiCorreo
{

    public class NewUserModel
    {
        public string customerGroup { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string documentType { get; set; }
        public string documentId { get; set; }
        public string phone { get; set; }
        public string cellPhone { get; set; }
        public AddressBase address { get; set; }
    }

}