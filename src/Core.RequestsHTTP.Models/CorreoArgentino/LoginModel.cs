using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.ApiMiCorreo
{
    public class LoginModel
    {
        public LoginModel(string email, string password)
        {
            this.email = email;
            this.password = password;
        }

        public string customerGroup { get; set; }
        public string email { get; set; }
        public string password { get; set;}
    }
}
