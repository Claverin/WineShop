using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WineShop.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:$###,###}")]
        public decimal Price { get; set; }
        [Display(Name = "Year Of Production")]
        public DateTime? YearOfProduction { get; set; }
        public string? Description { get; set; }
        [Required]
        public string Image { get; set; }

        [Required]
        [Display(Name = "Manufacturer Type")]
        public int IdManufacturer { get; set; }
        [ForeignKey("IdManufacturer")]
        public virtual Manufacturer? Manufacturer { get; set; }

        [Required]
        [Display(Name = "Product Type")]
        public int IdProductType { get; set; }
        [ForeignKey("IdProductType")]
        public virtual ProductType? ProductType { get; set; }
    }
}