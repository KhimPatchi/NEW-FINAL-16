using System.ComponentModel.DataAnnotations;

namespace SocialWelfarre.Models
{
    public class StockIn_DisasterKit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Stock must be greater than 0.")]
        public int Add_Stock1 { get; set; }

        [Display(Name = "Stock In Date")]
        public DateTime StockInDate { get; set; } = DateTime.Now;
    }
}
