using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.ApiMiCorreo
{
	public class LoginResponseModel
	{
		public string customerId { get; set; }
		public DateTime createdAt { get; set; }
		public string code { get; set; }
		public string message { get; set; }

	}
}