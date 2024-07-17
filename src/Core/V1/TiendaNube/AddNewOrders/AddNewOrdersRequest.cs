using System.Collections.Generic;
using Core.V1.TiendaNube.GetBranchOffices.Models;
using MediatR;
using Newtonsoft.Json;

namespace Core.V1.TiendaNube.AddNewOrders
{
    public class AddNewOrdersRequest : IRequest
    {

        public AddNewOrdersRequest()
        {
            this.id = new List<string>();
        }

        public int store { get; set; }

        public IEnumerable<string> id { get; set; }
    }


    public class AddNewOrderProcNextResponse
    {
        public string message { get; set; }
    };



    public class AddNewOrderProcNextRequest : IRequest<OrderStatus>
    {
        public int storeId { get; set; }
        public int orderId { get; set; }
    };

}
