using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OnlineQueueAPI.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace OnlineQueueAPI.Data
{
    public class OnlineQueueDbContext : DbContext
    {

        public OnlineQueueDbContext(DbContextOptions<OnlineQueueDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Queue> Queues { get; set; }
        public DbSet<UserOrganizationRole> UserOrganizationsRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Organization>()
                .HasIndex(o => o.Code)
                .IsUnique();

            modelBuilder.Entity<Organization>()
                .HasIndex(o => o.Hotline)
                .IsUnique();

            modelBuilder.Entity<Organization>()
                .HasIndex(o => o.Email)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //User - Appoinment (1 - n)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //Queue - Appointment (1 - n)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Queue)
                .WithMany(q => q.Appointments)
                .HasForeignKey(a => a.QueueId)
                .OnDelete(DeleteBehavior.Cascade);

            //Service - Queue (1 - n)
            modelBuilder.Entity<Queue>()
                .HasOne(q => q.Service)
                .WithMany(s => s.Queues)
                .HasForeignKey(q => q.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            //Organization - Service (1 - n)
            modelBuilder.Entity<Service>()
                .HasOne(s => s.Organization)
                .WithMany(o => o.Services)
                .HasForeignKey(s => s.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            //Field - Organization (1 - n)
            modelBuilder.Entity<Organization>()
                .HasOne(o => o.Field)
                .WithMany(f => f.Organizations)
                .HasForeignKey(o => o.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            //User - UserOrganizationRole (1 - n)
            modelBuilder.Entity<UserOrganizationRole>()
                .HasOne(uor => uor.User)
                .WithMany(u => u.UserOrganizationRoles)
                .HasForeignKey(uor => uor.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //Organization - UserOrganizationRole (1 - n)
            modelBuilder.Entity<UserOrganizationRole>()
                .HasOne(uor => uor.Organization)
                .WithMany(o => o.UserOrganizationRoles)
                .HasForeignKey(uor => uor.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            //User - Organization (N - N thông qua bảng trung gian UserOrganizationRole)
            modelBuilder.Entity<UserOrganizationRole>()
                .HasKey(uor => new { uor.UserId, uor.OrganizationId });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored));
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            using var transaction = await Database.BeginTransactionAsync(cancellationToken);

            foreach (var entry in ChangeTracker.Entries<ICreationInfo>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }

            try
            {
                var newAppointments = ChangeTracker.Entries<Appointment>()
                    .Where(e => e.State == EntityState.Added)
                    .Select(e => e.Entity);

                foreach (var appointment in newAppointments)
                {
                    var appointmentDate = appointment.ExpectedDateTime.Date;

                    int currentQueueNumber = await Appointments
                        .Where(a => a.QueueId == appointment.QueueId && a.ExpectedDateTime.Date == appointmentDate)
                        .OrderByDescending(a => a.QueueNumber)
                        .Select(a => a.QueueNumber)
                        .FirstOrDefaultAsync(cancellationToken);

                    appointment.QueueNumber = currentQueueNumber + 1;
                }

                var result = await base.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
