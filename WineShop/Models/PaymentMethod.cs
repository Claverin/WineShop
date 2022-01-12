using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WineShop.Models
{
    public class PaymentMethod
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}