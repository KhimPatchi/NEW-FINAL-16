using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialWelfarre.Models
{
    public class Consultation
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "First Name is required.")]
        public string First_Name2 { get; set; }
        [Required(ErrorMessage = "MiddleName is required.")]
        public string Middle_Name2 { get; set; }
        [Required(ErrorMessage = "Last Name is required.")]
        public string Last_Name2 { get; set; }
        public string FullName => $"{First_Name2} {Middle_Name2} {Last_Name2}";
        [Required(ErrorMessage = "ContactNumber is required.")]
        public string ContactNumber2 { get; set; }
        [Required(ErrorMessage = "Barangay Certificate is required.")]
        public string Brgy_Cert2 { get; set; }

        public string Brgy_Cert_Path2 { get; set; }
        [Required(ErrorMessage = "Proof Solo Parent is required.")]
        public string Proof_SoloParent2 { get; set; }

        public string Proof_SoloParent_Path2 { get; set; }
        [Required(ErrorMessage = "Child Birth Certificate  is required.")]
        public string Birth_Cert2 { get; set; }

        public string Birth_Cert_Path2 { get; set; }
        [Required(ErrorMessage = "Valid ID is required.")]
        public string Valid_ID2 { get; set; }

        public string Valid_ID_Path2 { get; set; }
        [Required(ErrorMessage = "1x1 Picture is required.")]
        public string x1_Pic2 { get; set; }
        [Required]
        public string x1_Pic_Path2 { get; set; }
        [Required(ErrorMessage = "Consultation Date is required.")]
        public DateOnly Consultation_Date { get; set; }
        [Required(ErrorMessage = "Consultation Time is required.")]
        public TimeOnly Consultation_Time { get; set; }
        public ActiveStatus2? Consultation_status { get; set; }
        public DateTime RequestDate { get; set; }
        public string? ApprovedById { get; set; } // Add the nullable operator ?
        [ForeignKey("ApprovedById")]
        public ApplicationUser? ApprovedBy { get; set; } // Make this nullable too

    }
    public enum ActiveStatus2
    {
        [Display(Name = "Pending")]
        Pending,
        [Display(Name = "Approved")]
        Approved,
        [Display(Name = "Denied")]
        Denied,
        [Display(Name = "Process")]
        Process
    }
}

