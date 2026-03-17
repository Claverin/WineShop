namespace WineShop.Models.ViewModels
{
    public class CartVM
    {
        public List<CartItemVM> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(x => x.LineTotal);
    }
}
