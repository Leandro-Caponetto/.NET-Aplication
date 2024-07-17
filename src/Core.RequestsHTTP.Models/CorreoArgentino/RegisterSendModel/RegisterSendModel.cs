using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.ApiMiCorreo.RegisterSendModel
{
    public class RegisterSendModel
    {
        public RegisterSendModel()
        {

        }


        public string customerGroup { get; set; }
        public string customerId { get; set; }
        public string extOrderId { get; set; }
        public string orderNumber { get; set; }
        public Sender sender { get; set; }
        public Recipient recipient { get; set; }
        public Shipping shipping { get; set; }

    }
}