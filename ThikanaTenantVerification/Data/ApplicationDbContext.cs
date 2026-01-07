using Microsoft.EntityFrameworkCore;
using ThikanaTenantVerification.Models;

namespace ThikanaTenantVerification.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Users
        public DbSet<User> Users { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<FamilyMember> FamilyMembers { get; set; }
        public DbSet<HouseWorker> HouseWorkers { get; set; }
        public DbSet<PreviousLandlord> PreviousLandlords { get; set; }
        public DbSet<CurrentLandlord> CurrentLandlords { get; set; }
        public DbSet<CurrentResidence> CurrentResidences { get; set; }

        // Verification
        public DbSet<VerificationLog> VerificationLogs { get; set; }
        public DbSet<PoliceVerificationRequest> PoliceVerificationRequests { get; set; }

        // Documents
        public DbSet<DocumentAttachment> DocumentAttachments { get; set; }

        // Notifications
        public DbSet<Notification> Notifications { get; set; }

        // OTP
        public DbSet<OTP> OTPs { get; set; }

        // Additional tables from your schema
        public DbSet<SystemAdmin> SystemAdmins { get; set; }
        public DbSet<Landlord> Landlords { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<MessageTemplate> MessageTemplates { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<VerificationRequest> VerificationRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.NIDNumber)
                .IsUnique()
                .HasFilter("[NIDNumber] IS NOT NULL");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.MobileNumber);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.VerificationStatus);

            // EmergencyContacts configuration
            modelBuilder.Entity<EmergencyContact>()
                .HasOne(ec => ec.User)
                .WithMany(u => u.EmergencyContacts)
                .HasForeignKey(ec => ec.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmergencyContact>()
                .HasIndex(ec => ec.UserId);

            // FamilyMembers configuration
            modelBuilder.Entity<FamilyMember>()
                .HasOne(fm => fm.User)
                .WithMany(u => u.FamilyMembers)
                .HasForeignKey(fm => fm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // HouseWorkers configuration
            modelBuilder.Entity<HouseWorker>()
                .HasOne(hw => hw.User)
                .WithMany(u => u.HouseWorkers)
                .HasForeignKey(hw => hw.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HouseWorker>()
                .HasIndex(hw => hw.NIDNumber);

            // CurrentLandlord configuration (1:1 with User)
            modelBuilder.Entity<CurrentLandlord>()
                .HasOne(cl => cl.User)
                .WithOne(u => u.CurrentLandlord)
                .HasForeignKey<CurrentLandlord>(cl => cl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CurrentLandlord>()
                .HasIndex(cl => cl.UserId)
                .IsUnique();

            // CurrentResidence configuration (1:1 with User)
            modelBuilder.Entity<CurrentResidence>()
                .HasOne(cr => cr.User)
                .WithOne(u => u.CurrentResidence)
                .HasForeignKey<CurrentResidence>(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CurrentResidence>()
                .HasIndex(cr => cr.UserId)
                .IsUnique();

            // VerificationLogs configuration
            modelBuilder.Entity<VerificationLog>()
                .HasOne(vl => vl.User)
                .WithMany(u => u.VerificationLogs)
                .HasForeignKey(vl => vl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VerificationLog>()
                .HasIndex(vl => vl.UserId);

            modelBuilder.Entity<VerificationLog>()
                .HasIndex(vl => vl.VerifiedAt);

            // DocumentAttachments configuration
            modelBuilder.Entity<DocumentAttachment>()
                .HasOne(da => da.User)
                .WithMany(u => u.DocumentAttachments)
                .HasForeignKey(da => da.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DocumentAttachment>()
                .HasIndex(da => da.UserId);

            // OTPs configuration
            modelBuilder.Entity<OTP>()
                .HasIndex(o => o.MobileNumber);

            modelBuilder.Entity<OTP>()
                .HasIndex(o => o.ExpiryTime);

            // SystemAdmin configuration
            modelBuilder.Entity<SystemAdmin>()
                .HasIndex(sa => sa.Email)
                .IsUnique();

            modelBuilder.Entity<SystemAdmin>()
                .HasIndex(sa => sa.AccessLevel);

            // Landlord configuration
            modelBuilder.Entity<Landlord>()
                .HasIndex(l => l.MobileNumber)
                .IsUnique();

            // SystemSettings configuration
            modelBuilder.Entity<SystemSetting>()
                .HasIndex(ss => ss.SettingKey)
                .IsUnique();

            // MessageTemplates configuration
            modelBuilder.Entity<MessageTemplate>()
                .HasIndex(mt => mt.TemplateName)
                .IsUnique();

            // AuditLogs configuration
            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => al.UserId);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => al.Entity);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(al => al.Timestamp);

            // VerificationRequests configuration
            modelBuilder.Entity<VerificationRequest>()
                .HasOne(vr => vr.User)
                .WithMany()
                .HasForeignKey(vr => vr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VerificationRequest>()
                .HasIndex(vr => vr.UserId);

            modelBuilder.Entity<VerificationRequest>()
                .HasIndex(vr => vr.Status);
        }
    }
}