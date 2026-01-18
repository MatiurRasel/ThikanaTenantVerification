using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ThikanaTenantVerification.Data;
using ThikanaTenantVerification.Services;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting Thikana Tenant Verification Application");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllersWithViews(options =>
    {
        // Add XML documentation support
        options.ReturnHttpNotAcceptable = true;
    }).AddRazorRuntimeCompilation();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Add session
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("SecuritySettings:SessionTimeoutMinutes", 30));
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

    // Add DbContext
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, b => b.MigrationsAssembly("ThikanaTenantVerification")));

    // Configure JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured in appsettings.json");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "ThikanaVerificationSystem",
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"] ?? "ThikanaUsers",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    // Configure Authorization with RBAC Policies
    builder.Services.AddAuthorization(options =>
    {
        // Tenant role policy
        options.AddPolicy("Tenant", policy => policy.RequireRole("Tenant"));
        
        // Landlord role policy
        options.AddPolicy("Landlord", policy => policy.RequireRole("Landlord"));
        
        // Police role policies
        options.AddPolicy("Police", policy => policy.RequireRole("Police", "Station", "Thana", "District", "Division"));
        options.AddPolicy("Station", policy => policy.RequireRole("Station"));
        options.AddPolicy("Thana", policy => policy.RequireRole("Thana", "District", "Division"));
        options.AddPolicy("District", policy => policy.RequireRole("District", "Division"));
        options.AddPolicy("Division", policy => policy.RequireRole("Division"));
        
        // Admin role policies
        options.AddPolicy("Admin", policy => policy.RequireRole("Admin", "Super"));
        options.AddPolicy("Super", policy => policy.RequireRole("Super"));
        
        // Combined policies
        options.AddPolicy("LandlordOrAdmin", policy => policy.RequireRole("Landlord", "Admin", "Super"));
        options.AddPolicy("PoliceOrAdmin", policy => policy.RequireRole("Police", "Station", "Thana", "District", "Division", "Admin", "Super"));
    });

    // Register Services
    builder.Services.AddScoped<IDataService, DataService>();
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<ILoggingService, LoggingService>();
    builder.Services.AddScoped<IApiMockService, ApiMockService>();
    builder.Services.AddScoped<IOtpService, OtpService>();

    // Add HTTP Context Accessor for accessing current user
    builder.Services.AddHttpContextAccessor();

    // Add Memory Cache
    builder.Services.AddMemoryCache();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSession();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    Log.Information("Application configured successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}