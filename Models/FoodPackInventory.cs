using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialWelfarre.Models
{
    public class FoodPackInventory
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Application ID")]
        public int ApplicationFoodPackId { get; set; }

        [Required]
        [Display(Name = "Stock In ID")]
        public int StockInId { get; set; }

        [Required]
        [Display(Name = "Total Packs")]
        public int TotalPacks { get; set; }

        [Required]
        [Display(Name = "Stock Left")]
        public int StockLeft { get; set; }

        [Required]
        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; }

        [Required]
        [Display(Name = "Barangay")]
        public Barangay? Barangay { get; set; }

        [Required]
        [Display(Name = "Number of Packs")]
        [Range(1, int.MaxValue)]
        public int Packs { get; set; }
    }

}