using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialWelfarre.Models
{
    public class DisasterKitInventory
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Stock In ID")]
        public int StockInId1 { get; set; }

        [Required]
        [Display(Name = "Total Packs")]
        public int TotalPacks1 { get; set; }

        [Required]
        [Display(Name = "Stock Left")]
        public int StockLeft { get; set; }

        [Required]
        [Display(Name = "Request Date")]
        public DateTime RequestDate1 { get; set; }

        [Required]
        public Barangay2 Barangay3 { get; set; }

        [Required]
        public Reason Reason3 { get; set; }

        [Required]
        [Display(Name = "Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }

        [Required]
        [Display(Name = "Transaction Time")]
        [DataType(DataType.Time)]
        public TimeSpan TransactionTime { get; set; }

        [Required]
        [Display(Name = "Number of Packs")]
        public int NumberOfPacks3 { get; set; }
    }

}