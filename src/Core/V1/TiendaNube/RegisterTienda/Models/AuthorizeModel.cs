using System;
using System.Collections.Immutable;
using System.Security.Claims;

namespace Core.V1.TiendaNube.RegisterTienda.Models
{
    public class AuthorizeModel
    {
        public AuthorizeModel(string code)
        {
            this.code = code ?? throw new ArgumentNullException(nameof(code));
            client_id = "2475";
            client_secret = "i0hUpObDG0QAMyotP4RXy6ZEYV12OVU35TrxRUqZ9t2CKtaf";
            grant_type = "authorization_code";
        }

        public string code { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
    }
}
