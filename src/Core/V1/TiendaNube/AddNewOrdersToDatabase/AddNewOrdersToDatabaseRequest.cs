using System;
using System.Collections.Generic;
using Core.Entities;
using Core.RequestsHTTP.Models.TiendaNube;
using Core.V1.TiendaNube.GetBranchOffices.Models;
using MediatR;
using Newtonsoft.Json;

namespace Core.V1.TiendaNube.AddNewOrdersToDatabase
{
    public class AddNewOrdersToDatabaseRequest : IRequest
    {
        public Guid seller_id { get; set; }
        public int seller_tnstoreid { get; set; }
        public string seller_causerid { get; set; }

        public IEnumerable<OrderResponseModel> Orders { get; set; }
    }
}
