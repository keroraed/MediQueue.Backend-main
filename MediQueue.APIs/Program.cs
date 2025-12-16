using MediQueue.APIs.Extensions;
using MediQueue.APIs.Middlewares;
using MediQueue.Core.Entities.Identity;
using MediQueue.Repository.Data;
using MediQueue.Repository.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(); // API Controllers
builder.Services.AddRazorPages(); // Razor Pages for Admin UI
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application Services
builder.Services.AddApplicationServices(builder.Configuration);

// Add Identity Services
builder.Services.AddIdentityServices(builder.Configuration);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithOrigins(
                  "http://localhost:4200",   // Angular frontend (HTTP)
                  "https://localhost:4200",  // Angular frontend (HTTPS)
                  "http://localhost:7100",   // API (HTTP)
                  "https://localhost:7101"   // API (HTTPS)
              );
    });
});

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    
    try
    {
        // Migrate and seed Identity database
        var identityContext = services.GetRequiredService<AppIdentityDbContext>();
        await identityContext.Database.MigrateAsync();
        
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await AppIdentityDbContextSeed.SeedUsersAsync(userManager, roleManager);
        
        // Migrate and seed Store database
        var storeContext = services.GetRequiredService<StoreContext>();
        await storeContext.Database.MigrateAsync();
        await StoreContextSeed.SeedAsync(storeContext);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred during migration");
    }
}

// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionMiddleware>();

app.UseStatusCodePagesWithReExecute("/errors/{0}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // Enable static files

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages(); // Razor Pages first
app.MapControllers(); // API Controllers second

app.Run();
