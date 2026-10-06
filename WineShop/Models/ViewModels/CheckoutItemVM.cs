namespace WineShop.Models.ViewModels
{
    public class CheckoutItemVM
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => Price * Quantity;
    }
}
