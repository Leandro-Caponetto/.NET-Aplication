using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace Core.V1.TiendaNube.AddNewOrders
{
    public class AddNewOrdersStatusRequest : IRequest
    {
        public int storeId { get; set; }

    }
}
