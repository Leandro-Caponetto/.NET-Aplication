using System;
using System.Collections.Generic;
using System.Text;

namespace Core.V1.TiendaNube.GetBranchOffices.Models
{
    
    public class ItemModel
    {
        public string name { get; set; }
        public string sku { get; set; }
        public int? quantity { get; set; }
        public bool free_shipping { get; set; }
        public int? grams { get; set; }
        public double price { get; set; }
        public Dimension dimensions { get; set; }
    }
}
