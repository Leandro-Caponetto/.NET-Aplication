
namespace Core.RequestsHTTP.Models.TiendaNube
{
    public class AuthorizeModel
    {
        public string code { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
    }
}
