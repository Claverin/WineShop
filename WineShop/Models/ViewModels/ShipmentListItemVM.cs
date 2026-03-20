namespace WineShop.Models.ViewModels
{
    public class ShipmentListItemVM
    {
        public int OrderId { get; set; }
        public string CustomerEmail { get; set; }
        public string StatusName { get; set; }
        public string PaymentMethodName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? Carrier { get; set; }
        public string? ShippingMethod { get; set; }
        public string? TrackingNumber { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
    }
}