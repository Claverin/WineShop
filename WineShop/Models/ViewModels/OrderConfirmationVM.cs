namespace WineShop.Models.ViewModels
{
    public class OrderConfirmationVM
    {
        public int OrderId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public decimal TotalAmount { get; set; }
        public string StatusName { get; set; }
    }
}