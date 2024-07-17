using System;

namespace Core.Entities
{
    public class OrderProduct
    {
        public OrderProduct() { }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int TNProductId { get; set; }
        public string TNProductName { get; set; }
        public decimal TNPrice { get; set; }
        public int TNQuantity { get; set; }
        public decimal TNWeight { get; set; }
        public decimal TNWidth { get; set; }
        public decimal TNHeight { get; set; }
        public decimal TNDepth { get; set; }
        public virtual Order Order{ get; set; }
        
    }
}
