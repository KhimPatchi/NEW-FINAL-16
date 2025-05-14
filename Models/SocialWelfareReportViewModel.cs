using SocialWelfarre.Models;
using System.Collections.Generic;
namespace SocialWelfare.Models.ViewModels
{
    public class SocialWelfareReportViewModel
    {
        public IEnumerable<ApplicationFoodPack> FoodPacks { get; set; }
        public IEnumerable<Certificate_Of_Indigency> Indigencies { get; set; }
        public IEnumerable<Consultation> Consultations { get; set; }
        public IEnumerable<AuditTrail> AuditTrails { get; set; }
        public IEnumerable<ApplicationUser> ApplicationUsers { get; set; }

    }
}