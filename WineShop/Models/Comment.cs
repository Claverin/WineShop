using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WineShop.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [Required]
        [Display(Name = "Comment")]
        public string CommentContent { get; set; }

        public string IdCustomer { get; set; }
        [ForeignKey("IdCustomer")]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        public int? IdProduct { get; set; }
        [ForeignKey("IdProduct")]
        public virtual Product? Product { get; set; }
    }
}