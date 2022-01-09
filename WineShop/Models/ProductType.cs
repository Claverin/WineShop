using System.ComponentModel.DataAnnotations;

namespace WineShop.Models
{
    public class ProductType
    {
        [Key]
        public int ID_ProductType { get; set; }
        [Required]
        public int Name { get; set; }
    }
}
