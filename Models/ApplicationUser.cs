using Microsoft.AspNetCore.Identity;

namespace SocialWelfarre.Models
{
    public class ApplicationUser: IdentityUser
    {   

        public int EmpNo { get; set; }
        public string First_Name { get; set; }
        public string Middle_Name { get; set; }
        public string Last_Name { get; set; }
        public string FullName => $"{First_Name} {Middle_Name} {Last_Name}";
        public int Age { get; set; }
        public bool IsMale { get; set; }
        public string Gender => IsMale ? "Male" : "Female";
        public DateOnly DateOfBirth { get; set; }
        public bool IsArchived { get; set; } = false;

    }
}
