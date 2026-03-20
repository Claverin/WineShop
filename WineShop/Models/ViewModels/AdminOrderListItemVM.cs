namespace WineShop.Models.ViewModels
{
    public class AdminOrderListItemVM
    {
        public int OrderId { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }
        public string StatusName { get; set; }
        public string PaymentMethodName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public bool HasShippingInfo { get; set; }
    }
}