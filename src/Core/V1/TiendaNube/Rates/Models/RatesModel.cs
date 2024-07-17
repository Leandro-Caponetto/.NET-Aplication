using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Claims;

namespace Core.V1.TiendaNube.Rates.Models
{
    public class RatesModel
    {
        public RatesModel()
        {
            this.Rates = new List<RateModel>();
        }

        public List<RateModel> Rates { get; set; }
    }
}
