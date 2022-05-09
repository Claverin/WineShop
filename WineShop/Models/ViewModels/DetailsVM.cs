namespace WineShop.Models.ViewModels
{
    public class DetailsVM
    {
        public DetailsVM()
        {
            Product = new Product();
        }
        public Product Product { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public Comment Comment { get; set; }
        public int UserRating { get; set; }
        public bool ExistsInCart { get; set; }
    }
}