using System;
using System.Collections.Generic;
using System.Text;

namespace Core.RequestsHTTP.Models.ApiMiCorreo
{

    public class DimensionLimits
    {
        public int minWeight { get; set; }
        public int maxWeight { get; set; }
        public int maxLength { get; set; }
        public int maxVolumetricLength { get; set; }
        public String validTo { get; set; }
    }



    public class Dimension
    {
        public int weight { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public int length { get; set; }
    }


    public class ShipPriceModel
    {
        public string customerGroup { get; set; }
        public string customerId { get; set; }
        public string postalCodeOrigin { get; set; }
        public string postalCodeDestination { get; set; }
        public string deliveredType { get; set; }
        public Dimension dimensions { get; set; }
    }

}