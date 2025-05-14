using System.ComponentModel.DataAnnotations;

namespace SocialWelfarre.Models
{
    public class DisasterKitTransaction
    {
        public int Id { get; set; }

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
        [Range(1, 100)]
        public int NumberOfPacks3 { get; set; }
    }
    public enum Barangay2
    {
        [Display(Name = "Barangay 1")]
        Barangay1,
        [Display(Name = "Barangay 2")]
        Barangay2,
        [Display(Name = "Barangay 3")]
        Barangay3,
        [Display(Name = "Barangay 4")]
        Barangay4,
        [Display(Name = "Barangay 5")]
        Barangay5,
        [Display(Name = "Barangay 6")]
        Barangay6,
        [Display(Name = "Barangay 7")]
        Barangay7,
        [Display(Name = "Barangay 8")]
        Barangay8,
        [Display(Name = "Barangay 9")]
        Barangay9,
        [Display(Name = "Barangay 10")]
        Barangay10
    }

}

