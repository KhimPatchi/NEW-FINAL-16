using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialWelfarre.Models
{
    public class ApplicationFoodPack
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "MiddleName is required.")]
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {MiddleName} {LastName}";
        [Required(ErrorMessage = "Number of packs must be at least 1.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of packs must be at least 1.")]
        public int Packs { get; set; }
        [Required(ErrorMessage = "Age must be at least 1.")]
        [Range(1, int.MaxValue, ErrorMessage = "Age must be at least 1.")]
        public int Age { get; set; }
        [Required(ErrorMessage = "Barangay is required.")]
        public Barangay? Barangay { get; set; }
        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }
        [Required(ErrorMessage = "ContactNumber is required.")]
        public string ContactNumber { get; set; }
        [Required(ErrorMessage = "Barangay Certificate is required.")]
        public string Brgy_Cert { get; set; }

        public string Brgy_Cert_Path { get; set; }
        [Required(ErrorMessage = "Valid is required.")]
        public string Valid_ID { get; set; }

        public string Valid_ID_Path { get; set; }
        [Required(ErrorMessage = "Reason is required.")]
        public Reason? Reason { get; set; }

        public ActiveStatus? Status { get; set; }

        public DateTime RequestDate { get; set; }

        public string? ApprovedById { get; set; } // Add the nullable operator ?
        [ForeignKey("ApprovedById")]
        public ApplicationUser? ApprovedBy { get; set; } // Make this nullable too
    }
    public enum Barangay
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

    public enum Reason
    {
        [Display(Name = "Hunger")]
        Hunger,
        [Display(Name = "Poverty")]
        Poverty,
        [Display(Name = "Unemployment")]
        Unemployment,
        [Display(Name = "Health Issues")]
        HealthIssues,
        [Display(Name = "Other Reasons")]
        Other
    }

    public enum ActiveStatus
    {
        [Display(Name = "Pending")]
        Pending,
        [Display(Name = "Approved")]
        Approved,
        [Display(Name = "Denied")]
        Denied,
        [Display(Name = "Processing")]
        Process
    }
}