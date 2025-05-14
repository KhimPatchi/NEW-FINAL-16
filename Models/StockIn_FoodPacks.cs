using System.ComponentModel.DataAnnotations;

namespace SocialWelfarre.Models
{
    public class StockIn_FoodPacks
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Added Stock")]
        [Range(1, int.MaxValue)]
        public int Add_Stock2 { get; set; }

        [Required]
        [Display(Name = "Restock DateTime")]
        public DateTime Restock_DateTime2 { get; set; }
    }
}

