using AutomationOfPurchases.API.Infrastructure.Seed;     // де лежить DatabaseSeeder
using AutomationOfPurchases.Shared.Models;               // де лежить AppDbContext, AppUser
using Microsoft.AspNetCore.Identity;                     // Identity
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutomationOfPurchases.API;
using AutomationOfPurchases.API.Services.Mappings;
using AutomationOfPurchases.API.Services;               // IRequestService, RequestService
using AutomationOfPurchases.API.Repositories;           // IUnitOfWork, UnitOfWork, IRepositoryFactory, RepositoryFactory
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------- CORS -----------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        // Вкажіть адреси вашого Blazor/клієнтського застосунку
        policy.WithOrigins("https://localhost:7018", "http://localhost:7018")
              .AllowAnyHeader()
              .AllowAnyMethod();
        // Якщо потрібно передавати Cookie чи Авторизацію з різних доменів:
        // policy.AllowCredentials();
    });
});

// ------------------------ 1) Зчитуємо рядок підключення ----------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

// ------------------------ 2) Налаштування DbContext --------------------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// ------------------ 3) Підключаємо ASP.NET Identity --------------------
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Тут можна налаштувати правила щодо паролів, блокування і т.д.
    // Наприклад:
    // options.Password.RequireDigit = false;
    // options.Password.RequiredLength = 4;
    // ...
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// --------------------- 4) Налаштування JWT ----------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(cfg =>
{
    var secretKey = builder.Configuration["JwtSettings:SecretKey"];
    var key = Encoding.UTF8.GetBytes(secretKey!);

    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        ValidateIssuer = false,   // або встановіть issuer, якщо треба
        ValidateAudience = false, // або встановіть audience, якщо треба
        ClockSkew = TimeSpan.Zero,

        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };
});

// --------------------- 5) Підключаємо AutoMapper ----------------------
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<UserProfile>();
    cfg.AddProfile<RequestProfile>();
    // інші профілі, якщо є
});

// --------------------- 6) Реєструємо свої сервіси ---------------------
// Тут ми реєструємо RepositoryFactory, UnitOfWork і RequestService
builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IDepartmentHeadService, DepartmentHeadService>();
builder.Services.AddScoped<IEconomistService, EconomistService>();

// --------------------- 7) Авторизація (політики) ----------------------
builder.Services.AddAuthorization();

// --------------------- 8) Swagger, контролери тощо --------------------
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Використовуємо IgnoreCycles для уникнення нескінченних циклів при серіалізації
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Створюємо й будуємо додаток
var app = builder.Build();

// --------------------- 9) Сідування (міграції + тестові користувачі) --
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // Викликаємо метод сидування
    await DatabaseSeeder.SeedTestDataAsync(dbContext, roleManager, userManager);
}

// -------------------- 10) Налаштування pipeline -----------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
