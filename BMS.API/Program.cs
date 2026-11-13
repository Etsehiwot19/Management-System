using BMS.Application.Interfaces;
using BMS.Infrastructure.Services;
using BMS.Infrastructure.Data;
using BMS.Infrastructure.Models;
using BMS.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
if (string.IsNullOrWhiteSpace(secretKey) || Encoding.UTF8.GetByteCount(secretKey) < 32)
    throw new InvalidOperationException("Configure JwtSettings:SecretKey with a random key of at least 32 bytes.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ILeaseService, LeaseService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<OverdueInvoiceBackgroundService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Building Management System API",
        Version = "v1",
        Description = "API for building operations, leases, invoicing, payments, reports, and notifications."
    });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed roles
// Seed roles and data
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Seed roles
    string[] roles = ["Admin", "Manager", "Viewer"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed units (7 floors, 3 units each)
    if (!context.Units.Any())
    {
        var units = new List<BMS.Infrastructure.Models.Unit>();
        for (int floor = 1; floor <= 7; floor++)
        {
            units.Add(new BMS.Infrastructure.Models.Unit { FloorNumber = floor, UnitNumber = $"{floor}A", UnitType = BMS.Application.Enums.UnitType.Shop, AreaSqMeters = 45, MonthlyRent = 5000 + (floor * 500), Status = BMS.Application.Enums.UnitStatus.Available, Description = $"Shop A on floor {floor}", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            units.Add(new BMS.Infrastructure.Models.Unit { FloorNumber = floor, UnitNumber = $"{floor}B", UnitType = BMS.Application.Enums.UnitType.Office, AreaSqMeters = 60, MonthlyRent = 7000 + (floor * 500), Status = BMS.Application.Enums.UnitStatus.Available, Description = $"Office B on floor {floor}", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            units.Add(new BMS.Infrastructure.Models.Unit { FloorNumber = floor, UnitNumber = $"{floor}C", UnitType = BMS.Application.Enums.UnitType.Shop, AreaSqMeters = 35, MonthlyRent = 4000 + (floor * 500), Status = BMS.Application.Enums.UnitStatus.Available, Description = $"Shop C on floor {floor}", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        }
        context.Units.AddRange(units);
        await context.SaveChangesAsync();
    }

    // Seed tenants
    if (!context.Tenants.Any())
    {
        var tenants = new List<BMS.Infrastructure.Models.Tenant>
        {
            new() { OrganizationName = "ABC Retail Ltd", TinNumber = "TIN001", Phone = "0911111111", BusinessType = BMS.Application.Enums.BusinessType.Retail, ContactPersonName = "John Smith", ContactEmail = "john@abc.com", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { OrganizationName = "XYZ Restaurant", TinNumber = "TIN002", Phone = "0922222222", BusinessType = BMS.Application.Enums.BusinessType.Restaurant, ContactPersonName = "Mary Jane", ContactEmail = "mary@xyz.com", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { OrganizationName = "Tech Solutions Inc", TinNumber = "TIN003", Phone = "0933333333", BusinessType = BMS.Application.Enums.BusinessType.Office, ContactPersonName = "Bob Tech", ContactEmail = "bob@tech.com", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { OrganizationName = "Health Plus Clinic", TinNumber = "TIN004", Phone = "0944444444", BusinessType = BMS.Application.Enums.BusinessType.Healthcare, ContactPersonName = "Dr. Sarah", ContactEmail = "sarah@health.com", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { OrganizationName = "EduCenter Academy", TinNumber = "TIN005", Phone = "0955555555", BusinessType = BMS.Application.Enums.BusinessType.Education, ContactPersonName = "Prof. Alan", ContactEmail = "alan@edu.com", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        context.Tenants.AddRange(tenants);
        await context.SaveChangesAsync();
    }

    // Seed leases once, using existing seeded units and tenants.
    if (!context.Leases.Any())
    {
        var units = await context.Units
            .Where(unit => unit.Status == BMS.Application.Enums.UnitStatus.Available)
            .OrderBy(unit => unit.Id)
            .Take(5)
            .ToListAsync();
        var tenants = await context.Tenants
            .Where(tenant => tenant.IsActive)
            .OrderBy(tenant => tenant.Id)
            .ToListAsync();

        if (units.Count > 0 && tenants.Count > 0)
        {
            var today = DateTime.UtcNow.Date;
            var endOffsets = new[] { 15, 45, 90, 150, 240 };
            var leases = units.Select((unit, index) => new BMS.Infrastructure.Models.Lease
            {
                UnitId = unit.Id,
                TenantId = tenants[index % tenants.Count].Id,
                StartDate = today.AddMonths(-6),
                EndDate = today.AddDays(endOffsets[index]),
                MonthlyRent = unit.MonthlyRent,
                DepositAmount = unit.MonthlyRent,
                Status = BMS.Application.Enums.LeaseStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            context.Leases.AddRange(leases);
            foreach (var unit in units)
            {
                unit.Status = BMS.Application.Enums.UnitStatus.Occupied;
                unit.UpdatedAt = DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
        }
    }
}

app.Run();
