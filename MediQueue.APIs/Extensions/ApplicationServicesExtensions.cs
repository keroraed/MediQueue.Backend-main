using MediQueue.APIs.Errors;
using MediQueue.Core.Repositories;
using MediQueue.Core.Services;
using MediQueue.Core.Settings;
using MediQueue.Repository;
using MediQueue.Repository.Data;
using MediQueue.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.APIs.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext for Store
      services.AddDbContext<StoreContext>(options =>
    {
 options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("MediQueue.Repository"));
      });

    // Add Memory Cache for performance optimization
        services.AddMemoryCache();

        // Add AutoMapper
  services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // Configure OtpSettings
    services.Configure<OtpSettings>(configuration.GetSection("OtpSettings"));

  // Register repositories
 services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
  services.AddScoped<IClinicRepository, ClinicRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IClinicRatingRepository, ClinicRatingRepository>();
        services.AddScoped<IClinicWorkingDayRepository, ClinicWorkingDayRepository>();
   services.AddScoped<IClinicExceptionRepository, ClinicExceptionRepository>();

   // Register services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IClinicService, ClinicService>();
      services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<IWorkingScheduleService, WorkingScheduleService>();
  services.AddScoped<IUserAddressService, UserAddressService>();
        
        // NEW: Register helper services following SOLID principles
        services.AddScoped<ITimeSlotService, TimeSlotService>();
     services.AddScoped<IAppointmentValidationService, AppointmentValidationService>();

        // Configure API behavior options
        services.Configure<ApiBehaviorOptions>(options =>
        {
      options.InvalidModelStateResponseFactory = actionContext =>
 {
        var errors = actionContext.ModelState
           .Where(e => e.Value.Errors.Count > 0)
        .SelectMany(x => x.Value.Errors)
      .Select(x => x.ErrorMessage).ToArray();

        var errorResponse = new ApiValidationErrorResponse
     {
   Errors = errors
 };

                return new BadRequestObjectResult(errorResponse);
   };
        });

        return services;
    }
}
