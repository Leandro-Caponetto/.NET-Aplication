using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Core.V1.TiendaNube.FormRegister
{
    public class FormRegisterRequest : IRequest<string>
    {
        public string Code { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
		public string Password { get; set; }
		public string DocumentType { get; set; }
		public string DocumentNumber { get; set; }
		public string Phone { get; set; }
		public string CellPhone { get; set; }
		public string Street { get; set; }
		public string StreetNumber { get; set; }
		public string Floor { get; set; }
		public string Depto { get; set; }
		public string Location { get; set; }
		public string City { get; set; }
		public string CPA { get; set; }
	}
}