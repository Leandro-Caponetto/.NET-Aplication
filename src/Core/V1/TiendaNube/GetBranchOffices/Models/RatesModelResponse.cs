using System;
using System.Collections.Generic;
using System.Text;

namespace Core.V1.TiendaNube.GetBranchOffices.Models
{
    public class RatesModelResponse
    {
        public RatesModelResponse()
        {
            this.rates = new List<RatesResponse>();
        }
        public List<RatesResponse> rates {get; set;}


    }
}
