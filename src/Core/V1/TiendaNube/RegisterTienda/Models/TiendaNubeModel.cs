using System;
using System.Collections.Immutable;
using System.Security.Claims;

namespace Core.V1.TiendaNube.RegisterTienda.Models
{
    public class TiendaNubeModel
    {
        public TiendaNubeModel(string token, string type, string scope , string id)
        {
            this.access_token = token;
            this.token_type = type;
            this.scope = scope;
            this.user_id = id;
        }

        public string access_token { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
        public string user_id { get; set; }
    }
}
