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
        public DateTime? SendDate { get; set; }
        public DateTime? DeliverDate { get; set; }
    }
}
