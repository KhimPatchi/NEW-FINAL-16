using System.ComponentModel;

namespace SocialWelfarre.Models
{
    public class RolesViewModel
    {
        [DisplayName("Role No")]
        public string Id { get; set; }
        [DisplayName("Role Name")]
        public string RoleName { get; set; }
    }
}
 