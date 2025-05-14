using System.ComponentModel.DataAnnotations;

namespace SocialWelfarre.Models
{
    public class RegisterViewModel

    {
        [Required] public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[\W_]).{12,}$", ErrorMessage = "Password must be at least 12 characters long, contain at least one uppercase letter and one symbol.")]
        public string Password { get; set; }
        [Required] public string First_Name { get; set; }
        public string Middle_Name { get; set; }
        [Required] public string Last_Name { get; set; }
        [Required] public int Age { get; set; }
        [Required] public bool IsMale { get; set; }
        [Required] public int EmpNo { get; set; }
        [Required] public DateOnly DateOfBirth { get; set; }
        [Required] public string Role { get; set; }
    }

}
