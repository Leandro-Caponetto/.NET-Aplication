using System;
using System.Collections.Generic;
using System.Text;

namespace Core.V1.TiendaNube.AddNewOrders
{
    public class OrderStatus
    {
        public OrderStatus(string _orderNumber, bool _berror, string _message)
        {
            orderNumber = _orderNumber;
            berror = _berror;
            message = _message;
        }

        public string orderNumber { get; set; }
        public bool berror { get; set; }
        public string message { get; set; }
    };

    public class AddNewOrderStatusResponse
    {
        public AddNewOrderStatusResponse()
        {
            this.orders = new List<OrderStatus>();
        }
        public List<OrderStatus> orders { get; set; }

    }
}
