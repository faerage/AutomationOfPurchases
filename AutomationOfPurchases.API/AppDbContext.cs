using AutomationOfPurchases.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutomationOfPurchases.API
{
    /// <summary>
    /// Контекст БД, що успадковується від IdentityDbContext для підтримки Identity-користувачів.
    /// </summary>
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Основні сутності:
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Request> Requests { get; set; } = null!;
        public DbSet<RequestItem> RequestItems { get; set; } = null!;
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<DeliveryRequest> DeliveryRequests { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<DepartmentEconomist> DepartmentEconomists { get; set; } = null!;

        // Списки для загальних/чистих потреб
        public DbSet<GeneralNeedsList> GeneralNeedsLists { get; set; } = null!;
        public DbSet<NetNeedsList> NetNeedsLists { get; set; } = null!;

        // **НОВІ** сутності для рядків загальних/чистих потреб:
        public DbSet<GeneralNeedsItem> GeneralNeedsItems { get; set; } = null!;
        public DbSet<NetNeedsItem> NetNeedsItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ===========================
            // 1) DepartmentEconomist composite key
            // ===========================
            builder.Entity<DepartmentEconomist>()
                .HasKey(de => new { de.DepartmentId, de.EconomistId });

            builder.Entity<DepartmentEconomist>()
                .HasOne(de => de.Department)
                .WithMany(d => d.DepartmentEconomists)
                .HasForeignKey(de => de.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DepartmentEconomist>()
                .HasOne(de => de.Economist)
                .WithMany(u => u.DepartmentEconomists)
                .HasForeignKey(de => de.EconomistId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===========================
            // 2) Department -> HeadOfDepartment
            // ===========================
            builder.Entity<Department>()
                .HasOne(d => d.HeadOfDepartment)
                .WithMany()
                .HasForeignKey(d => d.HeadOfDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // (Приклад, якщо у Department є колекція Economists)
            builder.Entity<Department>()
                .HasMany(d => d.Economists)
                .WithOne(u => u.Department)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===========================
            // 3) За бажання: налаштування зв’язків для GeneralNeedsList -> GeneralNeedsItem
            // ===========================
            // Якщо ви хочете явний Fluent API:
            // builder.Entity<GeneralNeedsItem>()
            //     .HasOne(gni => gni.List)
            //     .WithMany(gnl => gnl.Items)
            //     .HasForeignKey(gni => gni.GeneralNeedsListId)
            //     .OnDelete(DeleteBehavior.Cascade);

            // ===========================
            // 4) Так само для NetNeedsList -> NetNeedsItem (необов’язково):
            // ===========================
            // builder.Entity<NetNeedsItem>()
            //     .HasOne(nni => nni.List)
            //     .WithMany(nl => nl.Items)
            //     .HasForeignKey(nni => nni.NetNeedsListId)
            //     .OnDelete(DeleteBehavior.Cascade);

            // (Якщо не вказувати, EF спробує налаштувати все за конвенцією)
        }
    }
}
