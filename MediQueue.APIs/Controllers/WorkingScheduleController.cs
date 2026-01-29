using MediQueue.APIs.Errors;
using MediQueue.Core.DTOs;
using MediQueue.Core.Enums;
using MediQueue.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.APIs.Controllers;

public class WorkingScheduleController : BaseApiController
{
    private readonly IWorkingScheduleService _scheduleService;
    private readonly IClinicService _clinicService;

    public WorkingScheduleController(IWorkingScheduleService scheduleService, IClinicService clinicService)
    {
    _scheduleService = scheduleService;
        _clinicService = clinicService;
    }

    /// <summary>
    /// Get all working days for clinic by clinic ID (Public)
    /// </summary>
    [HttpGet("{clinicId}/working-days")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ClinicWorkingDayDto>>> GetWorkingDays(int clinicId)
    {
        var workingDays = await _scheduleService.GetWorkingDaysAsync(clinicId);
        return Ok(workingDays);
    }

    /// <summary>
    /// Get current clinic's working days (Clinic only)
    /// </summary>
    [HttpGet("my-working-days")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<List<ClinicWorkingDayDto>>> GetMyWorkingDays()
    {
        var clinicId = await GetCurrentClinicIdAsync();
        var workingDays = await _scheduleService.GetWorkingDaysAsync(clinicId);
        return Ok(workingDays);
    }

    /// <summary>
    /// Get working day by clinic ID and day of week (Public)
    /// </summary>
    [HttpGet("{clinicId}/working-days/{dayOfWeek}")]
    [AllowAnonymous]
    public async Task<ActionResult<ClinicWorkingDayDto>> GetWorkingDay(int clinicId, DayOfWeekEnum dayOfWeek)
    {
        var workingDay = await _scheduleService.GetWorkingDayAsync(clinicId, dayOfWeek);
        
        if (workingDay == null)
            return NotFound(new ApiResponse(404, "Working day not found"));

        return Ok(workingDay);
    }

    /// <summary>
    /// Bulk update all working days (Clinic only)
    /// </summary>
    [HttpPut("working-days")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<List<ClinicWorkingDayDto>>> BulkUpdateWorkingDays(BulkUpdateWorkingDaysDto dto)
    {
        try
     {
        var clinicId = await GetCurrentClinicIdAsync();
            var workingDays = await _scheduleService.BulkUpdateWorkingDaysAsync(clinicId, dto);
 return Ok(workingDays);
        }
   catch (KeyNotFoundException ex)
     {
     return NotFound(new ApiResponse(404, ex.Message));
      }
        catch (ArgumentException ex)
        {
   return BadRequest(new ApiResponse(400, ex.Message));
   }
    }

    /// <summary>
    /// Update a specific working day (Clinic only)
    /// </summary>
    [HttpPut("working-days/{id}")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<ClinicWorkingDayDto>> UpdateWorkingDay(int id, UpdateClinicWorkingDayDto dto)
    {
      try
        {
    var workingDay = await _scheduleService.UpdateWorkingDayAsync(id, dto);
       return Ok(workingDay);
 }
        catch (KeyNotFoundException ex)
        {
     return NotFound(new ApiResponse(404, ex.Message));
        }
        catch (ArgumentException ex)
{
            return BadRequest(new ApiResponse(400, ex.Message));
        }
    }

    /// <summary>
    /// Get all exceptions for clinic by clinic ID (Public)
    /// </summary>
    [HttpGet("{clinicId}/exceptions")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ClinicExceptionDto>>> GetExceptions(int clinicId)
    {
        var exceptions = await _scheduleService.GetExceptionsAsync(clinicId);
        return Ok(exceptions);
    }

    /// <summary>
    /// Get current clinic's exceptions (Clinic only)
    /// </summary>
    [HttpGet("my-exceptions")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<List<ClinicExceptionDto>>> GetMyExceptions()
    {
        var clinicId = await GetCurrentClinicIdAsync();
        var exceptions = await _scheduleService.GetExceptionsAsync(clinicId);
        return Ok(exceptions);
    }

    /// <summary>
    /// Add exception date (Clinic only)
    /// </summary>
    [HttpPost("exceptions")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<ClinicExceptionDto>> AddException(CreateClinicExceptionDto dto)
    {
        try
  {
            var clinicId = await GetCurrentClinicIdAsync();
var exception = await _scheduleService.AddExceptionAsync(clinicId, dto);
  return CreatedAtAction(nameof(GetExceptions), new { id = exception.Id }, exception);
        }
        catch (KeyNotFoundException ex)
        {
   return NotFound(new ApiResponse(404, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
     return BadRequest(new ApiResponse(400, ex.Message));
        }
    }

    /// <summary>
    /// Update exception (Clinic only)
    /// </summary>
    [HttpPut("exceptions/{id}")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<ClinicExceptionDto>> UpdateException(int id, UpdateClinicExceptionDto dto)
  {
  try
        {
 var exception = await _scheduleService.UpdateExceptionAsync(id, dto);
   return Ok(exception);
        }
        catch (KeyNotFoundException ex)
        {
   return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    /// <summary>
    /// Delete exception (Clinic only)
    /// </summary>
    [HttpDelete("exceptions/{id}")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult> DeleteException(int id)
    {
    try
     {
          await _scheduleService.DeleteExceptionAsync(id);
            return Ok(new ApiResponse(200, "Exception deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
  return NotFound(new ApiResponse(404, ex.Message));
     }
    }

    /// <summary>
    /// Get clinic working schedule with exceptions (Public)
    /// Allows patients to view clinic operating hours before booking
    /// </summary>
    [HttpGet("{clinicId}/schedule")]
    [AllowAnonymous]
    public async Task<ActionResult<ClinicScheduleDto>> GetClinicSchedule(int clinicId)
    {
        try
        {
            var workingDays = await _scheduleService.GetWorkingDaysAsync(clinicId);
            var exceptions = await _scheduleService.GetExceptionsAsync(clinicId);
            
            var schedule = new ClinicScheduleDto
            {
                ClinicId = clinicId,
                WorkingDays = workingDays,
                Exceptions = exceptions.Where(e => e.ExceptionDate >= DateTime.UtcNow.Date).ToList()
            };
            
            return Ok(schedule);
        }
        catch (Exception)
        {
            return NotFound(new ApiResponse(404, "Clinic not found"));
        }
    }

    /// <summary>
    /// Check if clinic is available on a specific date (Public)
    /// </summary>
    [HttpGet("{clinicId}/available")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> IsClinicAvailable(int clinicId, [FromQuery] DateTime date)
    {
        var isAvailable = await _scheduleService.IsClinicAvailableAsync(clinicId, date);
        return Ok(new { clinicId, date = date.Date, isAvailable });
    }

    /// <summary>
    /// Get daily capacity for a specific date (Public)
    /// </summary>
    [HttpGet("{clinicId}/capacity")]
    [AllowAnonymous]
 public async Task<ActionResult<int>> GetDailyCapacity(int clinicId, [FromQuery] DateTime date)
    {
        var capacity = await _scheduleService.GetDailyCapacityAsync(clinicId, date);
        return Ok(new { clinicId, date = date.Date, capacity });
    }

    private async Task<int> GetCurrentClinicIdAsync()
    {
        var userId = GetCurrentUserId();
        var clinic = await _clinicService.GetClinicByUserIdAsync(userId);
    
     if (clinic == null)
        throw new InvalidOperationException("Clinic profile not found for this user");

        return clinic.Id;
    }
}
