using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutomationOfPurchases.Shared.Models
{
    // Замість простого DbContext робимо:
    //   IdentityDbContext<AppUser>  (або <AppUser, IdentityRole, string>)
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Ваші решта сутностей
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Request> Requests { get; set; } = null!;
        public DbSet<RequestItem> RequestItems { get; set; } = null!;
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<DeliveryRequest> DeliveryRequests { get; set; } = null!;
        public DbSet<GeneralNeedsList> GeneralNeedsLists { get; set; } = null!;
        public DbSet<NetNeedsList> NetNeedsLists { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Якщо у вас є зв’язки Department -> HeadOfDepartment
            // або Department -> Economists, налаштуйте їх тут:
            // (приклад)
            builder.Entity<Department>()
                .HasOne(d => d.HeadOfDepartment)
                .WithMany()
                .HasForeignKey(d => d.HeadOfDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Department>()
                .HasMany(d => d.Economists)
                .WithOne(u => u.Department)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
