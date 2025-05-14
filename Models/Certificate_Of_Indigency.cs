using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialWelfarre.Models
{
    public class Certificate_Of_Indigency
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "First Name  is required.")]
        public string FirstName1 { get; set; }
        [Required(ErrorMessage = "Middle Name  is required.")]
        public string MiddleName1 { get; set; }
        [Required(ErrorMessage = "Last Name  is required.")]
        public string LastName1 { get; set; }
        public string FullName => $"{FirstName1} {MiddleName1} {LastName1}";
        [Range(1, int.MaxValue, ErrorMessage = "Age must be at least 1.")]
        [Required(ErrorMessage = "Age  is required.")]
        public int Age1 { get; set; }
        [Required(ErrorMessage = "Barangay is required.")]
        public Barangay1? Barangay1 { get; set; }
        [Required(ErrorMessage = "No.Request is required.")]
        public int No_Rquested1 { get; set; }
        [Required(ErrorMessage = "Adress is required.")]
        public string Address1 { get; set; }
        [Required(ErrorMessage = "ContactNumber is required.")]
        public string ContactNumber1 { get; set; }
        [Required(ErrorMessage = "Barangay Cert is required.")]
        public string Brgy_Cert1 { get; set; }

        public string Brgy_Cert_Path1 { get; set; }
        [Required(ErrorMessage = "ContactNumber is required.")]
        public string Valid_ID1 { get; set; }

        public string Valid_ID_Path1 { get; set; }
        [Required(ErrorMessage = "Reason is required.")]
        public Reason1? Reason1 { get; set; }
        public ActiveStatus1? Status1 { get; set; }
        public DateTime RequestDate1 { get; set; }

        public string? ApprovedById { get; set; } // Add the nullable operator ?
        [ForeignKey("ApprovedById")]
        public ApplicationUser? ApprovedBy { get; set; } // Make this nullable too

    }
    public enum Barangay1
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

    public enum Reason1
    {
        [Display(Name = "Educational Assistance")]
        Educational_Assistance,
        [Display(Name = "Medical Assistance")]
        Medical_Assistance,
        [Display(Name = "Government Aid or Social Welfare Programs")]
        Government_Aid_or_Social_Welfare_ProgramsUnemployment,
        [Display(Name = "Employment Application")]
        Employment_Application,
        [Display(Name = "Legal or Court Purposes")]
        Legal_or_Court_Purposes,
        [Display(Name = "Burial or Funeral Assistance")]
        Burial_or_Funeral_Assistance,
        [Display(Name = "Others")]
        Others
    }

    public enum ActiveStatus1
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

