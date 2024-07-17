
namespace Core.RequestsHTTP.Models.TiendaNube
{
    public class AuthorizeResponseModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
        public string user_id { get; set; }
    }
}
