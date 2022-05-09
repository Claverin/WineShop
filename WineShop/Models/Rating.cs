using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WineShop.Models
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int RatingValue { get; set; }

        public string IdCustomer { get; set; }
        [ForeignKey("IdCustomer")]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        public int IdProduct { get; set; }
        [ForeignKey("IdProduct")]
        public virtual Product Product { get; set; }
    }
}