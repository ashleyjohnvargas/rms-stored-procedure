using Microsoft.EntityFrameworkCore;

namespace PMS.Models
{
    public class ApplicationDbContext : DbContext   
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<PropertyManager> PropertyManagers { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Lease> Leases { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<UnitImage> UnitImages { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default values
            modelBuilder.Entity<Lease>()
                .Property(l => l.LeaseStatus)
                .HasDefaultValue("Active");

            modelBuilder.Entity<MaintenanceRequest>()
                .Property(mr => mr.RequestStatus)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentStatus)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Unit>()
                .Property(u => u.UnitStatus)
                .HasDefaultValue("Active");

            modelBuilder.Entity<Unit>()
                .Property(u => u.AvailabilityStatus)
                .HasDefaultValue("Available");

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Tenant>()
                .Property(t => t.IsActualTenant)
                .HasDefaultValue(false);
        }
    }
}
