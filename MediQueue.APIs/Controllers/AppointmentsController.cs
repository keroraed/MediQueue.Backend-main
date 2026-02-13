using MediQueue.APIs.Errors;
using MediQueue.Core.DTOs;
using MediQueue.Core.Repositories;
using MediQueue.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.APIs.Controllers;

[Authorize]
public class AppointmentsController : BaseApiController
{
    private readonly IAppointmentService _appointmentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClinicService _clinicService;

    public AppointmentsController(IAppointmentService appointmentService, IUnitOfWork unitOfWork, IClinicService clinicService)
    {
        _appointmentService = appointmentService;
        _unitOfWork = unitOfWork;
        _clinicService = clinicService;
    }

    /// <summary>
    /// Get next available date for a clinic (Public)
    /// </summary>
    [HttpGet("next-available")]
    [AllowAnonymous]
    public async Task<ActionResult<NextAvailableDateDto>> GetNextAvailableDate([FromQuery] int clinicId, [FromQuery] DateTime? fromDate)
    {
        try
        {
     var startDate = fromDate ?? DateTime.UtcNow.Date;
       var nextAvailable = await _appointmentService.GetNextAvailableDateAsync(clinicId, startDate);
       return Ok(nextAvailable);
  }
        catch (KeyNotFoundException ex)
     {
            return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    /// <summary>
    /// Book a new appointment (Patient only) - AUTO-QUEUE SYSTEM
    /// Patient selects date only, backend auto-assigns next available time slot
    /// Requires: clinicId, appointmentDate
    /// Returns: Appointment with auto-assigned time and queue number
    /// </summary>
    [HttpPost("book")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<AppointmentDto>> BookAppointment(BookAppointmentDto dto)
    {
     try
        {
    var patientId = GetCurrentUserId();
     var appointment = await _appointmentService.BookAppointmentAsync(patientId, dto);
  return Ok(appointment);
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
    /// Get appointment details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDetailsDto>> GetAppointmentDetails(int id)
    {
        var appointment = await _appointmentService.GetAppointmentDetailsAsync(id);
      
        if (appointment == null)
    return NotFound(new ApiResponse(404, "Appointment not found"));

return Ok(appointment);
}

    /// <summary>
/// Cancel an appointment (Patient only)
/// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult> CancelAppointment(int id)
    {
    try
        {
            var userId = GetCurrentUserId();
       await _appointmentService.CancelAppointmentAsync(id, userId);
 return Ok(new ApiResponse(200, "Appointment canceled successfully"));
        }
        catch (KeyNotFoundException ex)
        {
        return NotFound(new ApiResponse(404, ex.Message));
      }
        catch (UnauthorizedAccessException ex)
     {
            return Forbid();
}
  catch (InvalidOperationException ex)
        {
         return BadRequest(new ApiResponse(400, ex.Message));
        }
    }

    /// <summary>
    /// Update appointment status (Clinic only)
    /// Validates: Clinic ownership, valid state transitions
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<AppointmentDto>> UpdateAppointmentStatus(int id, UpdateAppointmentStatusDto dto)
    {
  try
        {
     var clinicUserId = GetCurrentUserId();
 var updatedAppointment = await _appointmentService.UpdateAppointmentStatusAsync(id, clinicUserId, dto);
            return Ok(updatedAppointment);
        }
     catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
        }
      catch (UnauthorizedAccessException ex)
        {
    return StatusCode(403, new ApiResponse(403, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
   return BadRequest(new ApiResponse(400, ex.Message));
        }
    }

    /// <summary>
 /// Mark appointment as in progress (Clinic only)
    /// </summary>
    [HttpPost("{id}/start")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<AppointmentDto>> MarkInProgress(int id)
    {
        try
        {
 var clinicUserId = GetCurrentUserId();
         var updatedAppointment = await _appointmentService.MarkInProgressAsync(id, clinicUserId);
       return Ok(updatedAppointment);
    }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
        }
     catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ApiResponse(403, ex.Message));
      }
        catch (InvalidOperationException ex)
        {
      return BadRequest(new ApiResponse(400, ex.Message));
        }
    }

    /// <summary>
    /// Mark appointment as completed (Clinic only)
    /// </summary>
    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<AppointmentDto>> MarkCompleted(int id)
    {
        try
        {
  var clinicUserId = GetCurrentUserId();
            var updatedAppointment = await _appointmentService.MarkCompletedAsync(id, clinicUserId);
            return Ok(updatedAppointment);
        }
        catch (KeyNotFoundException ex)
{
      return NotFound(new ApiResponse(404, ex.Message));
     }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ApiResponse(403, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(400, ex.Message));
   }
    }

    /// <summary>
    /// Mark appointment as delayed (Clinic only)
    /// </summary>
    [HttpPost("{id}/delay")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<AppointmentDto>> MarkDelayed(int id)
  {
        try
      {
            var clinicUserId = GetCurrentUserId();
var updatedAppointment = await _appointmentService.MarkDelayedAsync(id, clinicUserId);
            return Ok(updatedAppointment);
        }
   catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
   {
     return StatusCode(403, new ApiResponse(403, ex.Message));
      }
        catch (InvalidOperationException ex)
        {
        return BadRequest(new ApiResponse(400, ex.Message));
  }
    }

    /// <summary>
    /// Call next patient in queue (Clinic only)
    /// </summary>
    [HttpPost("clinic/call-next")]
    [Authorize(Roles = "Clinic")]
 public async Task<ActionResult<AppointmentDto>> CallNextPatient([FromQuery] DateTime date)
    {
try
        {
  var clinicUserId = GetCurrentUserId();
       var clinic = await _clinicService.GetClinicByUserIdAsync(clinicUserId);
            if (clinic == null)
 return NotFound(new ApiResponse(404, "Clinic profile not found"));
   
 var appointment = await _appointmentService.CallNextPatientAsync(clinic.Id, date);
      return Ok(appointment);
        }
  catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse(400, ex.Message));
        }
    }

    /// <summary>
    /// Get clinic queue for a specific date (Clinic only)
    /// </summary>
    [HttpGet("clinic/queue")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<ClinicQueueDto>> GetClinicQueue([FromQuery] DateTime date)
    {
        try
        {
          var clinicUserId = GetCurrentUserId();
            var clinic = await _clinicService.GetClinicByUserIdAsync(clinicUserId);
    if (clinic == null)
       return NotFound(new ApiResponse(404, "Clinic profile not found"));
            
     var queue = await _appointmentService.GetClinicQueueAsync(clinic.Id, date);
            return Ok(queue);
      }
        catch (KeyNotFoundException ex)
        {
       return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    /// <summary>
    /// Get clinic 7-day queue view (Clinic only)
  /// Shows daily summaries for the next 7 days starting from startDate
    /// </summary>
    [HttpGet("clinic/weekly-queue")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<WeeklyQueueDto>> GetClinicWeeklyQueue([FromQuery] DateTime? startDate)
    {
try
 {
   var clinicUserId = GetCurrentUserId();
            var clinic = await _clinicService.GetClinicByUserIdAsync(clinicUserId);
            if (clinic == null)
       return NotFound(new ApiResponse(404, "Clinic profile not found"));
          
     var start = startDate ?? DateTime.UtcNow.Date;
     var weeklyQueue = await _appointmentService.GetClinicWeeklyQueueAsync(clinic.Id, start);
  return Ok(weeklyQueue);
        }
        catch (KeyNotFoundException ex)
        {
      return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    /// <summary>
    /// Get patient's appointment history (Patient only)
    /// </summary>
    [HttpGet("patient/history")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<PatientAppointmentHistoryDto>> GetPatientHistory()
    {
        var patientId = GetCurrentUserId();
     var history = await _appointmentService.GetPatientHistoryAsync(patientId);
      return Ok(history);
    }

    /// <summary>
  /// Get patient's upcoming appointments (Patient only)
/// </summary>
    [HttpGet("patient/upcoming")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<List<AppointmentDto>>> GetPatientUpcomingAppointments()
    {
        var patientId = GetCurrentUserId();
        var appointments = await _appointmentService.GetPatientUpcomingAppointmentsAsync(patientId);
  return Ok(appointments);
    }

    /// <summary>
    /// Get patient's past appointments (Patient only)
    /// </summary>
    [HttpGet("patient/past")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<List<AppointmentDto>>> GetPatientPastAppointments()
    {
        var patientId = GetCurrentUserId();
        var appointments = await _appointmentService.GetPatientPastAppointmentsAsync(patientId);
        return Ok(appointments);
    }

    /// <summary>
    /// Get estimated wait time for an appointment
    /// </summary>
    [HttpGet("{id}/wait-time")]
    public async Task<ActionResult<int>> GetEstimatedWaitTime(int id)
    {
        var waitTime = await _appointmentService.GetEstimatedWaitTimeAsync(id);
      return Ok(new { appointmentId = id, estimatedWaitMinutes = waitTime });
    }
}
