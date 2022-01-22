using Microsoft.AspNetCore.Mvc.Rendering;

namespace WineShop.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }
        public IEnumerable<SelectListItem> ManufacturerSelectList { get; set; }
        public IEnumerable<SelectListItem> ProductTypeSelectList { get; set; }
    }
}