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
        public DbSet<LeaseDetails> LeaseDetails { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<UnitImage> UnitImages { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Request> Requests { get; set; }

        // View for Pending Leases
        public DbSet<PendingLeaseViewModel> PendingLeasesView { get; set; }

        // View for Active Leases
        public DbSet<LeaseViewModel> LeaseView { get; set; }

        // View for Payments page
        public DbSet<ManagerPaymentViewModel> ManagerPaymentView { get; set; }

        // View for Requests page
        public DbSet<PMRequestView> PMRequestViews { get; set; }

        // View for Staffs page
        public DbSet<PMStaffView> PMStaffView { get; set; }

        // View for Units page
        public DbSet<UnitViewModel> ActiveUnits { get; set; }

        // View for Units page in Prospective Tenant    
        public DbSet<PTenantUnitViewModel> PTenantUnitsView { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default values
            modelBuilder.Entity<Lease>()
                .Property(l => l.LeaseStatus)
                .HasDefaultValue("Pending");

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

            modelBuilder.Entity<Staff>()
                .Property(s => s.IsVacant)
                .HasDefaultValue(true);

            modelBuilder.Entity<Request>()
                .Property(r => r.RequestStatus)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<PendingLeaseViewModel>()
               .HasNoKey() // Specify that this is a keyless entity since it's based on a view.
               .ToView("PendingLeasesView"); // Link the model to the database view.

            modelBuilder.Entity<LeaseViewModel>()
                .HasNoKey() // Specify that this entity does not have a primary key.
                .ToView("VW_PMActiveLease"); // Map it to the database view.

            modelBuilder.Entity<ManagerPaymentViewModel>(entity =>
            {
                entity.HasNoKey(); // No primary key since it's a view
                entity.ToView("ManagerPaymentView"); // Specify the view name in the database
            });

            modelBuilder.Entity<PMRequestView>()
                .HasNoKey()
                .ToView("RMS_VW_PMRequestView");

            modelBuilder.Entity<PMStaffView>()
                .HasNoKey()
                .ToView("RMS_VW_PMStaff");

            modelBuilder.Entity<UnitViewModel>(entity =>
            {
                entity.HasNoKey();  // The view doesn't have a primary key
                entity.ToView("RMS_VW_LIST_OF_UNITS");  // Map the view to the UnitViewModel
            });

            modelBuilder.Entity<PTenantUnitViewModel>()
                .HasNoKey()
                .ToView("RMS_VW_PTenantUnits");

        }
    }
}
