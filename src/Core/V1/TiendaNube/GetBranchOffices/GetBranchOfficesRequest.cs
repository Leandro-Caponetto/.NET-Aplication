using System.Collections.Generic;
using Core.V1.TiendaNube.GetBranchOffices.Models;
using MediatR;

namespace Core.V1.TiendaNube.GetBranchOffices
{
    public class GetBranchOfficesRequest : IRequest<RatesModelResponse>
    {

        public GetBranchOfficesRequest()
        {
            this.items = new List<ItemModel>();
        }

        public int store_id { get; set; }
        public string currency { get; set; }
        public string language { get; set; }
        public OriginAndDestinationModel origin { get; set; }
        public OriginAndDestinationModel destination { get; set; }
        public IEnumerable<ItemModel> items { get; set; }
    }

}
