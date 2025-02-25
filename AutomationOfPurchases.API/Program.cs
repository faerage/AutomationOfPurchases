using AutomationOfPurchases.API.Infrastructure.Seed;     // <- простір імен, де лежить DatabaseSeeder
using AutomationOfPurchases.Shared.Models;               // <- простір імен, де лежить AppDbContext, AppUser
using Microsoft.AspNetCore.Identity;                     // <- Identity
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        // Укажіть адреси клієнта, наприклад, http://localhost:7018 або https://localhost:7018
        policy.WithOrigins("https://localhost:7018", "http://localhost:7018")
              .AllowAnyHeader()
              .AllowAnyMethod();
        // Якщо потрібно передавати Cookie чи Авторизацію з різних доменів, додайте:
        // .AllowCredentials();
    });
});

// 1) Зчитуємо рядок підключення з appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

// 2) Налаштування DbContext (EF Core з PostgreSQL чи іншим SQL)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// 3) Підключаємо ASP.NET Identity (AppUser + IdentityRole)
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Тут можна налаштувати правила щодо паролів, блокування і т.д.
    // options.Password.RequireDigit = false;
    // ...
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 4) Налаштування JWT-аутентифікації
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(cfg =>
{
    // Беремо секретний ключ із конфігурації
    var secretKey = builder.Configuration["JwtSettings:SecretKey"];
    var key = Encoding.UTF8.GetBytes(secretKey!);

    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        ValidateIssuer = false,   // або встановіть issuer, якщо треба
        ValidateAudience = false, // або встановіть audience, якщо треба
        ClockSkew = TimeSpan.Zero
    };
});

// 5) Авторизація (якщо потрібні політики)
builder.Services.AddAuthorization();

// 6) Swagger, контролери, інші сервіси
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Створюємо й будуємо додаток
var app = builder.Build();

// 7) Створюємо scope і викликаємо Seeder (міграції + тестові користувачі)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // Викликаємо метод сидування
    await DatabaseSeeder.SeedTestDataAsync(dbContext, roleManager, userManager);
}

// 8) Використовуємо аутентифікацію та авторизацію
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
