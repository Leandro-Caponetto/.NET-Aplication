using MediatR;

namespace Core.V1.TiendaNube.LoginCA
{
    public class LoginCARequest : IRequest<string>
    {
        public string Code { get; set; }
        public string Username { get; set; }// en el  form el name tienen q llamrase asi y el id
        public string Password { get; set; }
    }
}
