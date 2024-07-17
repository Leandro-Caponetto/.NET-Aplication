using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.ApiMiCorreo
{
    public class RegisterSendResponseModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public DateTimeOffset createdAt { get; set; }

    }
}
