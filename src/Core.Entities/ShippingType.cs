using System;

namespace Core.Entities
{
    public class ShippingType
    {
        public ShippingType() { }

        public int Id { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }

    }
}