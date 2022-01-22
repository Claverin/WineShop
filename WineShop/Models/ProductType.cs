using System.ComponentModel.DataAnnotations;

namespace WineShop.Models
{
    public class ProductType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}