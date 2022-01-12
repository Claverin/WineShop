using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WineShop.Models
{
    public class Shipment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Send date")]
        public DateTime SendDate { get; set; }
        [DisplayName("Deliver date")]
        public DateTime? DeliverDate { get; set; }
        public string Details { get; set; }
    }
}