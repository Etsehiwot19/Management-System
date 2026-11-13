using BMS.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BMS.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Unit> Units { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<LegalDocument> LegalDocuments { get; set; }
        public DbSet<Lease> Leases { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Soft delete filters - automatically exclude deleted records
            modelBuilder.Entity<Unit>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Tenant>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<Lease>().HasQueryFilter(l => !l.IsDeleted);

            // Indexes for performance
            modelBuilder.Entity<Tenant>()
                .HasIndex(t => t.TinNumber).IsUnique();
            modelBuilder.Entity<Unit>()
                .HasIndex(u => u.UnitNumber).IsUnique();
            modelBuilder.Entity<Lease>()
                .HasIndex(l => l.Status);
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.Status);
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.PaymentDate);

            // Only one active lease per unit at a time
            modelBuilder.Entity<Lease>()
                .HasIndex(l => new { l.UnitId, l.Status })
                .IsUnique()
                .HasFilter("[Status] = 0");
        }
    }
}
