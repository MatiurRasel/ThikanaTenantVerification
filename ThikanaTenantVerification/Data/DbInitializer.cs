using Microsoft.EntityFrameworkCore;
using ThikanaTenantVerification.Models;

namespace ThikanaTenantVerification.Data
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(ApplicationDbContext context, ILogger<DbInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Initialize()
        {
            try
            {
                // Apply pending migrations
                _context.Database.Migrate();

                // Create database if it doesn't exist
                _context.Database.EnsureCreated();

                // Seed data if tables are empty
                SeedData();

                _logger.LogInformation("Database initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        private void SeedData()
        {
            // Seed SystemSettings if empty
            if (!_context.SystemSettings.Any())
            {
                _context.SystemSettings.AddRange(
                    new SystemSetting
                    {
                        SettingKey = "OTPExpiryMinutes",
                        SettingValue = "5",
                        Description = "OTP expiration time in minutes",
                        Category = "Security"
                    },
                    new SystemSetting
                    {
                        SettingKey = "VerificationThreshold",
                        SettingValue = "90",
                        Description = "Minimum completion percentage for verification",
                        Category = "Verification"
                    },
                    new SystemSetting
                    {
                        SettingKey = "DefaultThana",
                        SettingValue = "ধানমন্ডি",
                        Description = "Default thana for new registrations",
                        Category = "Location"
                    },
                    new SystemSetting
                    {
                        SettingKey = "DefaultDistrict",
                        SettingValue = "ঢাকা",
                        Description = "Default district for new registrations",
                        Category = "Location"
                    }
                );
            }

            // Seed MessageTemplates if empty
            if (!_context.MessageTemplates.Any())
            {
                _context.MessageTemplates.AddRange(
                    new MessageTemplate
                    {
                        TemplateName = "OTPVerification",
                        TemplateType = "SMS",
                        BodyBN = "আপনার OTP কোড: {otp}. এটি {minutes} মিনিটের জন্য বৈধ।",
                        Variables = "{otp},{minutes}"
                    },
                    new MessageTemplate
                    {
                        TemplateName = "RegistrationSuccess",
                        TemplateType = "SMS",
                        BodyBN = "স্বাগতম {name}! আপনার রেজিস্ট্রেশন সফলভাবে সম্পন্ন হয়েছে।",
                        Variables = "{name}"
                    }
                );
            }

            // Seed Super Admin if empty
            if (!_context.SystemAdmins.Any())
            {
                _context.SystemAdmins.Add(new SystemAdmin
                {
                    Name = "সুপার এডমিন",
                    Email = "admin@thikana.gov.bd",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Designation = "সুপার এডমিনিস্ট্রেটর",
                    MobileNumber = "01700000000",
                    AccessLevel = "Super"
                });
            }

            // Seed Sample Landlord if empty
            if (!_context.Landlords.Any())
            {
                _context.Landlords.Add(new Landlord
                {
                    NameBN = "মোঃ রফিকুল ইসলাম",
                    MobileNumber = "01712345678",
                    Email = "rafiq@example.com",
                    Address = "১২৩, গুলশান, ঢাকা"
                });
            }

            _context.SaveChanges();
        }
    }
}
