using MediatR;

namespace Core.V1.TiendaNube.RegisterTienda
{
    public class RegisterTiendaRequest : IRequest
    {
        public string Code { get; set; }
    }
}
