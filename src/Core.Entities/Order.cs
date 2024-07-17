using System;
using System.Collections.Generic;

namespace Core.Entities
{
    public class Order
    {
        public Order() { }

        public Guid Id { get; set; }
        public Guid SellerId { get; set; }//
        public int TNOrderId { get; set; }
        public int TNOrderStatusId { get; set; }
        public string CAIdClientId { get; set; }
        public DateTimeOffset TNCreatedOn { get; set; }
        public DateTimeOffset TNUpdatedOn { get; set; }
        public int ShippingTypeId { get; set; }
        public string CAOrderId { get; set; }
        public string SenderName { get; set; }
        public string SenderPhone { get; set; }
        public string SenderCellPhone { get; set; }
        public string SenderStreet { get; set; }
        public int SenderHeight { get; set; }
        public string SenderFloor { get; set; }
        public string SenderDpto { get; set; }
        public string SenderLocality { get; set; }
        public int? SenderProvinceId { get; set; }
        public string SenderPostalCode { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverCellPhone { get; set; }
        public string ReceiverMail { get; set; }
        public string ReceiverCASucursal { get; set; }
        public string ReceiverStreet { get; set; }
        public int ReceiverHeight { get; set; }
        public string ReceiverFloor { get; set; }
        public string ReceiverDpto { get; set; }
        public string ReceiverLocality { get; set; }
        public int? ReceiverProvinceId { get; set; }
        public string ReceiverPostalCode { get; set; }
        public decimal TNTotalWeight { get; set; }
        public decimal TNTotalPrice { get; set; }
        public decimal TotalHeight { get; set; }
        public decimal TotalWidth { get; set; }
        public decimal TotalDepth { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public virtual Seller Seller { get; set; }
        public virtual TNOrderStatus TNOrderStatus { get; set; }
        public virtual ShippingType ShippingType { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
        
    }
}