using MediQueue.APIs.Errors;
using MediQueue.Core.DTOs;
using MediQueue.Core.Entities.Identity;
using MediQueue.Core.Repositories;
using MediQueue.Repository.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.APIs.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppIdentityDbContext _identityContext;
    private readonly IWebHostEnvironment _env;

    public AdminController(
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        AppIdentityDbContext identityContext,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _identityContext = identityContext;
        _env = env;
    }

    /// <summary>
    /// Get system statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<SystemStatsDto>> GetSystemStats()
    {
      try
        {
 var allClinics = await _unitOfWork.Clinics.GetAllAsync();
   var totalClinics = allClinics.Count();
            
var totalPatients = await _userManager.Users.CountAsync(u => u.EmailConfirmed);
      
   var allAppointments = await _unitOfWork.Repository<Core.Entities.Appointment>().GetAllAsync();
 var totalAppointments = allAppointments.Count();
            var todayAppointments = allAppointments.Count(a => a.AppointmentDate.Date == DateTime.UtcNow.Date);

   return Ok(new SystemStatsDto
 {
 TotalClinics = totalClinics,
          TotalPatients = totalPatients,
      TotalAppointments = totalAppointments,
    TodayAppointments = todayAppointments
   });
        }
      catch (Exception ex)
 {
    return BadRequest(new ApiResponse(400, $"Error getting stats: {ex.Message}"));
 }
    }

    /// <summary>
    /// Get recent appointments
    /// </summary>
    [HttpGet("appointments/recent")]
    public async Task<ActionResult<List<RecentAppointmentDto>>> GetRecentAppointments()
    {
        try
        {
            var appointments = await _unitOfWork.Repository<Core.Entities.Appointment>()
                .GetAllAsync();
            
            var clinics = await _unitOfWork.Clinics.GetAllAsync();
            var appUsers = await _userManager.Users.ToListAsync();
            
            var recentAppointments = appointments
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new RecentAppointmentDto
                {
                    Id = a.Id,
                    PatientName = appUsers.FirstOrDefault(u => u.Id == a.PatientId)?.DisplayName ?? "Unknown Patient",
                    ClinicName = clinics.FirstOrDefault(c => c.Id == a.ClinicId)?.DoctorName ?? "Unknown Clinic",
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status.ToString()
                })
                .ToList();

            return Ok(recentAppointments);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(400, $"Error getting appointments: {ex.Message}"));
        }
    }

    /// <summary>
  /// Get all users with filters
  /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetUsers([FromQuery] string? role, [FromQuery] string? status)
    {
  try
   {
        var users = await _userManager.Users.ToListAsync();

    var userDtos = new List<UserDto>();
        foreach (var user in users)
            {
     var userRoles = await _userManager.GetRolesAsync(user);
             var userRole = userRoles.FirstOrDefault() ?? "Patient";

    // Apply role filter
     if (!string.IsNullOrEmpty(role) && role != "all" && userRole != role)
      continue;

     // Apply status filter
       if (!string.IsNullOrEmpty(status) && status != "all")
     {
            if (status == "locked" && !await _userManager.IsLockedOutAsync(user))
 continue;
         if (status == "active" && await _userManager.IsLockedOutAsync(user))
           continue;
    }

     userDtos.Add(new UserDto
     {
  Id = user.Id,
    Email = user.Email!,
   DisplayName = user.DisplayName,
           PhoneNumber = user.PhoneNumber,
       Role = userRole,
     IsEmailConfirmed = user.EmailConfirmed,
      IsLocked = await _userManager.IsLockedOutAsync(user),
 DateCreated = user.DateCreated,
         LastLoginDate = user.LastLoginDate
         });
   }

  return Ok(userDtos);
        }
  catch (Exception ex)
     {
       return BadRequest(new ApiResponse(400, $"Error getting users: {ex.Message}"));
}
    }

    /// <summary>
    /// Lock user account
/// </summary>
    [HttpPost("users/{userId}/lock")]
    public async Task<ActionResult> LockUserAccount(string userId)
    {
        try
        {
       var user = await _userManager.FindByIdAsync(userId);
   if (user == null)
          return NotFound(new ApiResponse(404, "User not found"));

       // Don't allow locking admin accounts
       var roles = await _userManager.GetRolesAsync(user);
  if (roles.Contains("Admin"))
    return BadRequest(new ApiResponse(400, "Cannot lock admin accounts"));

  await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
    
   return Ok(new ApiResponse(200, "User account locked successfully"));
    }
   catch (Exception ex)
      {
  return BadRequest(new ApiResponse(400, $"Error locking account: {ex.Message}"));
    }
    }

    /// <summary>
    /// Unlock user account
/// </summary>
    [HttpPost("users/{userId}/unlock")]
    public async Task<ActionResult> UnlockUserAccount(string userId)
    {
        try
   {
            var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
         return NotFound(new ApiResponse(404, "User not found"));

   await _userManager.SetLockoutEndDateAsync(user, null);
            
 return Ok(new ApiResponse(200, "User account unlocked successfully"));
        }
        catch (Exception ex)
  {
     return BadRequest(new ApiResponse(400, $"Error unlocking account: {ex.Message}"));
        }
    }

 /// <summary>
    /// Delete user
/// </summary>
  [HttpDelete("users/{userId}")]
    public async Task<ActionResult> DeleteUser(string userId)
    {
     try
        {
            var user = await _userManager.FindByIdAsync(userId);
   if (user == null)
      return NotFound(new ApiResponse(404, "User not found"));

   // Don't allow deleting admin accounts
   var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
     return BadRequest(new ApiResponse(400, "Cannot delete admin accounts"));

            var result = await _userManager.DeleteAsync(user);
      if (!result.Succeeded)
  return BadRequest(new ApiResponse(400, "Failed to delete user"));
   
            return Ok(new ApiResponse(200, "User deleted successfully"));
        }
catch (Exception ex)
        {
  return BadRequest(new ApiResponse(400, $"Error deleting user: {ex.Message}"));
        }
    }

    /// <summary>
    /// Create a new clinic account (Admin only).
    /// Accepts multipart/form-data so a profile picture can be uploaded.
    /// </summary>
    [HttpPost("clinics")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> AddClinic([FromForm] AdminCreateClinicDto dto)
    {
        using var identityTransaction = await _identityContext.Database.BeginTransactionAsync();
        try
        {
            // 1. Create Identity user
            var user = new AppUser
            {
                DisplayName = dto.DisplayName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DateCreated = DateTime.UtcNow,
                EmailConfirmed = true // Admin-created clinics are pre-verified
            };

            var identityResult = await _userManager.CreateAsync(user, dto.Password);
            if (!identityResult.Succeeded)
            {
                var duplicate = identityResult.Errors.FirstOrDefault(e =>
                    e.Code == "DuplicateUserName" || e.Code == "DuplicateEmail");

                if (duplicate != null)
                    return BadRequest(new ApiResponse(400, "Email address is already in use"));

                return BadRequest(new ApiResponse(400,
                    string.Join(", ", identityResult.Errors.Select(e => e.Description))));
            }

            await _userManager.AddToRoleAsync(user, "Clinic");
            await identityTransaction.CommitAsync();

            // 2. Handle profile picture upload
            string? profilePictureUrl = null;
            if (dto.ProfilePicture != null && dto.ProfilePicture.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "clinic-pictures");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(dto.ProfilePicture.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfilePicture.CopyToAsync(stream);
                }

                profilePictureUrl = $"/uploads/clinic-pictures/{fileName}";
            }

            // 3. Create business User entity
            try
            {
                var businessUser = new Core.Entities.User
                {
                    Email = dto.Email,
                    PasswordHash = user.PasswordHash!,
                    Role = "Clinic",
                    IsVerified = true,
                    IsActive = true
                };
                _unitOfWork.Users.Add(businessUser);
                await _unitOfWork.Complete();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
            {
                // Already exists – ignore
            }

            // 4. Create Clinic Profile
            var clinicProfile = new Core.Entities.ClinicProfile
            {
                AppUserId = user.Id,
                DoctorName = dto.DoctorName,
                Specialty = dto.Specialty,
                Description = dto.Description,
                SlotDurationMinutes = dto.SlotDurationMinutes,
                ProfilePictureUrl = profilePictureUrl,
                ConsultationFee = dto.ConsultationFee,
                PaymentMethods = dto.PaymentMethods != null && dto.PaymentMethods.Any()
                    ? string.Join(",", dto.PaymentMethods)
                    : null
            };
            _unitOfWork.Clinics.Add(clinicProfile);
            await _unitOfWork.Complete();

            // 5. Create Clinic Address
            var clinicAddress = new Core.Entities.ClinicAddress
            {
                ClinicId = clinicProfile.Id,
                Country = dto.Country,
                City = dto.City,
                Area = dto.Area,
                Street = dto.Street,
                Building = dto.Building,
                Notes = dto.AddressNotes
            };
            _unitOfWork.Repository<Core.Entities.ClinicAddress>().Add(clinicAddress);
            await _unitOfWork.Complete();

            // 6. Create additional phones if provided
            if (dto.AdditionalPhones != null && dto.AdditionalPhones.Any())
            {
                foreach (var phone in dto.AdditionalPhones)
                {
                    _unitOfWork.Repository<Core.Entities.ClinicPhone>().Add(
                        new Core.Entities.ClinicPhone { ClinicId = clinicProfile.Id, PhoneNumber = phone });
                }
                await _unitOfWork.Complete();
            }

            return Ok(new ApiResponse(200, "Clinic created successfully"));
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
        {
            await identityTransaction.RollbackAsync();
            return BadRequest(new ApiResponse(400, "Email address or phone number is already in use"));
        }
        catch (Exception)
        {
            await identityTransaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Get all clinics
    /// </summary>
  [HttpGet("clinics")]
    public async Task<ActionResult<List<ClinicListDto>>> GetClinics()
    {
        try
  {
      var clinics = await _unitOfWork.Clinics.GetAllAsync();
  var users = await _userManager.Users.ToListAsync();
    
     var clinicDtos = new List<ClinicListDto>();
foreach (var clinic in clinics)
  {
     // Use eager-loaded navigation properties
     var user = users.FirstOrDefault(u => u.Id == clinic.AppUserId);
        
     // Use already loaded relationships
            var avgRating = clinic.Ratings.Any() ? clinic.Ratings.Average(r => r.Rating) : (double?)null;
        
     clinicDtos.Add(new ClinicListDto
     {
   Id = clinic.Id,
 DoctorName = clinic.DoctorName,
       Specialty = clinic.Specialty,
     Description = clinic.Description,
    Email = user?.Email ?? "N/A",
        PhoneNumber = user?.PhoneNumber,
   City = clinic.Address?.City,
        ProfilePictureUrl = clinic.ProfilePictureUrl,
        ConsultationFee = clinic.ConsultationFee,
        PaymentMethods = string.IsNullOrEmpty(clinic.PaymentMethods)
            ? new List<string>()
            : clinic.PaymentMethods.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
        AverageRating = avgRating,
      TotalRatings = clinic.Ratings.Count,
         TotalAppointments = clinic.Appointments?.Count ?? 0,
    CreatedDate = user?.DateCreated ?? DateTime.UtcNow
 });
   }

        return Ok(clinicDtos);
        }
  catch (Exception ex)
   {
            return BadRequest(new ApiResponse(400, $"Error getting clinics: {ex.Message}"));
 }
    }

    /// <summary>
    /// Get all appointments
    /// </summary>
    [HttpGet("appointments/all")]
    public async Task<ActionResult<List<AppointmentListDto>>> GetAllAppointments()
    {
        try
        {
            var appointments = await _unitOfWork.Repository<Core.Entities.Appointment>().GetAllAsync();
            var clinics = await _unitOfWork.Clinics.GetAllAsync();
            var appUsers = await _userManager.Users.ToListAsync();

            var appointmentDtos = appointments.Select(a => new AppointmentListDto
            {
                Id = a.Id,
                PatientName = appUsers.FirstOrDefault(u => u.Id == a.PatientId)?.DisplayName ?? "Unknown Patient",
                ClinicName = clinics.FirstOrDefault(c => c.Id == a.ClinicId)?.DoctorName ?? "Unknown Clinic",
                AppointmentDate = a.AppointmentDate,
                QueueNumber = a.QueueNumber,
                Status = a.Status.ToString(),
                CreatedAt = a.CreatedAt
            }).ToList();

            return Ok(appointmentDtos);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(400, $"Error getting appointments: {ex.Message}"));
        }
    }

    /// <summary>
    /// Delete clinic
    /// </summary>
    [HttpDelete("clinics/{id}")]
    public async Task<ActionResult> DeleteClinic(int id)
    {
        try
        {
            var clinic = await _unitOfWork.Clinics.GetByIdAsync(id);
            if (clinic == null)
                return NotFound(new ApiResponse(404, "Clinic not found"));

            _unitOfWork.Clinics.Delete(clinic);
            await _unitOfWork.Complete();
          
            return Ok(new ApiResponse(200, "Clinic deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(400, $"Error deleting clinic: {ex.Message}"));
        }
    }
}

// DTOs
public class SystemStatsDto
{
    public int TotalClinics { get; set; }
  public int TotalPatients { get; set; }
    public int TotalAppointments { get; set; }
    public int TodayAppointments { get; set; }
}

public class RecentAppointmentDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
  public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
  public string? PhoneNumber { get; set; }
  public string Role { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }
    public bool IsLocked { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? LastLoginDate { get; set; }
}

public class ClinicListDto
{
    public int Id { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public decimal? ConsultationFee { get; set; }
    public List<string> PaymentMethods { get; set; } = new();
    public double? AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public int TotalAppointments { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class AppointmentListDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
  public int QueueNumber { get; set; }
    public string Status { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
}
