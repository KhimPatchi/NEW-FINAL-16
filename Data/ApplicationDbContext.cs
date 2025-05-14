using SocialWelfarre.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialWelfarre.Models;
using System.Reflection.Emit;


namespace SocialWelfarre.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        {

        }

        public DbSet<DisasterKitTransaction> DisasterKitTransactions { get; set; }
        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<Certificate_Of_Indigency> Certificate_Of_Indigencies { get; set; }
        public DbSet<ApplicationFoodPack> ApplicationFoodPack { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }
        public DbSet<FoodPackInventory> FoodPackInventories { get; set; }
        public DbSet<DisasterKitInventory> DisasterKitInventories { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>()
       .HasIndex(u => u.NormalizedEmail)
       .IsUnique();



            builder.Entity<ApplicationFoodPack>().HasKey(e => e.Id);
            builder.Entity<Certificate_Of_Indigency>().HasKey(e => e.Id);
            builder.Entity<Consultation>().HasKey(e => e.Id);
        }
        public DbSet<FoodPackInventory> FoodPackInventory { get; set; }
        public DbSet<SocialWelfarre.Models.StockIn_FoodPacks> StockIn_FoodPacks { get; set; } = default!;
        public DbSet<SocialWelfarre.Models.StockIn_DisasterKit> StockIn_DisasterKit { get; set; } = default!;


    }
}
