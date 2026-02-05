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

    public AppointmentsController(IAppointmentService appointmentService, IUnitOfWork unitOfWork)
    {
        _appointmentService = appointmentService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Book a new appointment (Patient only)
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
  catch (InvalidOperationException ex)
        {
  return BadRequest(new ApiResponse(400, ex.Message));
 }
    }

    /// <summary>
    /// Update appointment status (Clinic only)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<AppointmentDto>> UpdateAppointmentStatus(int id, UpdateAppointmentStatusDto dto)
    {
        try
        {
            var appUserId = GetCurrentUserId();
            var clinicId = await GetClinicIdFromUserIdAsync(appUserId);
            if (clinicId == null)
                return NotFound(new ApiResponse(404, "Clinic profile not found"));
            
            // Authorization: Verify clinic owns this appointment
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null)
                return NotFound(new ApiResponse(404, "Appointment not found"));
            
            if (appointment.ClinicId != clinicId.Value)
                return Forbid();
            
            var updatedAppointment = await _appointmentService.UpdateAppointmentStatusAsync(id, dto);
            return Ok(updatedAppointment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
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
            var appUserId = GetCurrentUserId();
            var clinicId = await GetClinicIdFromUserIdAsync(appUserId);
            if (clinicId == null)
                return NotFound(new ApiResponse(404, "Clinic profile not found"));
            
            // Authorization: Verify clinic owns this appointment
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null)
                return NotFound(new ApiResponse(404, "Appointment not found"));
            
            if (appointment.ClinicId != clinicId.Value)
                return Forbid();
            
            var updatedAppointment = await _appointmentService.MarkInProgressAsync(id);
            return Ok(updatedAppointment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
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
            var appUserId = GetCurrentUserId();
            var clinicId = await GetClinicIdFromUserIdAsync(appUserId);
            if (clinicId == null)
                return NotFound(new ApiResponse(404, "Clinic profile not found"));
            
            // Authorization: Verify clinic owns this appointment
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null)
                return NotFound(new ApiResponse(404, "Appointment not found"));
            
            if (appointment.ClinicId != clinicId.Value)
                return Forbid();
            
            var updatedAppointment = await _appointmentService.MarkCompletedAsync(id);
            return Ok(updatedAppointment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
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
            var appUserId = GetCurrentUserId();
            var clinicId = await GetClinicIdFromUserIdAsync(appUserId);
            if (clinicId == null)
                return NotFound(new ApiResponse(404, "Clinic profile not found"));
            
            // Authorization: Verify clinic owns this appointment
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null)
                return NotFound(new ApiResponse(404, "Appointment not found"));
            
            if (appointment.ClinicId != clinicId.Value)
                return Forbid();
            
            var updatedAppointment = await _appointmentService.MarkDelayedAsync(id);
            return Ok(updatedAppointment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
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
            var appUserId = GetCurrentUserId();
            var clinicId = await GetClinicIdFromUserIdAsync(appUserId);
            if (clinicId == null)
                return NotFound(new ApiResponse(404, "Clinic profile not found"));
            
            var appointment = await _appointmentService.CallNextPatientAsync(clinicId.Value, date);
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
            var appUserId = GetCurrentUserId();
            var clinicId = await GetClinicIdFromUserIdAsync(appUserId);
            if (clinicId == null)
                return NotFound(new ApiResponse(404, "Clinic profile not found"));
            
            var queue = await _appointmentService.GetClinicQueueAsync(clinicId.Value, date);
            return Ok(queue);
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

    /// <summary>
    /// Get all clinic appointments (Clinic only)
    /// Returns all appointments for the clinic regardless of date
    /// </summary>
  [HttpGet("clinic/all")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<List<AppointmentDto>>> GetAllClinicAppointments()
    {
 try
        {
 var appUserId = GetCurrentUserId();
            var clinicId = await GetClinicIdFromUserIdAsync(appUserId);
            if (clinicId == null)
            return NotFound(new ApiResponse(404, "Clinic profile not found"));
     
          var appointments = await _unitOfWork.Appointments.GetAllClinicAppointmentsAsync(clinicId.Value);
  
     // Map to DTOs
            var appointmentDtos = appointments.Select(apt => new AppointmentDto
            {
 Id = apt.Id,
           ClinicId = apt.ClinicId,
          ClinicName = apt.Clinic?.DoctorName ?? "Unknown",
       DoctorName = apt.Clinic?.DoctorName ?? "Unknown",
Specialty = apt.Clinic?.Specialty ?? "Unknown",
 PatientId = apt.PatientId,
    AppointmentDate = apt.AppointmentDate,
        QueueNumber = apt.QueueNumber,
       Status = apt.Status,
      StatusName = apt.Status.ToString(),
          EstimatedWaitMinutes = 0,
    CreatedAt = apt.CreatedAt
            }).OrderByDescending(a => a.AppointmentDate).ToList();
          
            return Ok(appointmentDtos);
 }
        catch (Exception ex)
  {
    return BadRequest(new ApiResponse(400, $"Error getting appointments: {ex.Message}"));
        }
    }

    private async Task<int?> GetClinicIdFromUserIdAsync(string appUserId)
    {
        var clinic = await _unitOfWork.Clinics.GetClinicByUserIdAsync(appUserId);
        return clinic?.Id;
    }
}
