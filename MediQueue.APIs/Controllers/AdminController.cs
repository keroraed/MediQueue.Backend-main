using MediQueue.APIs.Errors;
using MediQueue.Core.Entities.Identity;
using MediQueue.Core.Repositories;
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

    public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
    {
   _userManager = userManager;
     _unitOfWork = unitOfWork;
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
