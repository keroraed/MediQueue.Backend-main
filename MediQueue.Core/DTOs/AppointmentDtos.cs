using System.ComponentModel.DataAnnotations;
using MediQueue.Core.Enums;

namespace MediQueue.Core.DTOs;

// ==================== Appointment DTOs ====================

public class AppointmentDto
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public required string ClinicName { get; set; }
    public required string DoctorName { get; set; }
    public required string Specialty { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string? PatientName { get; set; }
    public string? PatientPhone { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; } // Auto-assigned by backend
    public int QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; }
  public required string StatusName { get; set; }
    public int EstimatedWaitMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BookAppointmentDto
{
    [Required]
    public int ClinicId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }
    
    // REMOVED: AppointmentTime - Backend auto-assigns next available slot
}

public class UpdateAppointmentStatusDto
{
    [Required]
    public AppointmentStatus Status { get; set; }
}

public class AppointmentDetailsDto
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public required string ClinicName { get; set; }
    public required string DoctorName { get; set; }
    public required string Specialty { get; set; }
    public ClinicAddressDto? Address { get; set; }
    public List<ClinicPhoneDto> Phones { get; set; } = new();
    public string PatientId { get; set; } = string.Empty;
    public string? PatientName { get; set; }
    public string? PatientPhone { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; } // Auto-assigned
    public int QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; }
    public required string StatusName { get; set; }
    public int EstimatedWaitMinutes { get; set; }
  public int CurrentQueueNumber { get; set; }
    public int PeopleAhead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ClinicQueueDto
{
    public int ClinicId { get; set; }
    public required string ClinicName { get; set; }
    public required string DoctorName { get; set; }
    public DateTime Date { get; set; }
    public int CurrentQueueNumber { get; set; }
    public int TotalAppointments { get; set; }
    public int BookedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int CanceledCount { get; set; }
public List<AppointmentDto> Appointments { get; set; } = new();
}

public class PatientAppointmentHistoryDto
{
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CanceledAppointments { get; set; }
    public List<AppointmentDto> Appointments { get; set; } = new();
}

// ==================== 7-Day Queue View DTOs ====================

public class WeeklyQueueDto
{
    public int ClinicId { get; set; }
    public required string ClinicName { get; set; }
    public required string DoctorName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<DailyQueueSummaryDto> DailySummaries { get; set; } = new();
    public int TotalAppointments { get; set; }
    public List<string> BusyDays { get; set; } = new();
}

public class DailyQueueSummaryDto
{
    public DateTime Date { get; set; }
    public required string DayName { get; set; }
    public int TotalAppointments { get; set; }
    public int BookedCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int CanceledCount { get; set; }
    public int MaxCapacity { get; set; }
    public int AvailableSlots { get; set; }
    public bool IsWorkingDay { get; set; }
    public bool HasException { get; set; }
  public string? ExceptionReason { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}

// ==================== Next Available Date Response ====================

public class NextAvailableDateDto
{
    public DateTime? Date { get; set; }
    public string? DayName { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int AvailableSlots { get; set; }
    public string? Message { get; set; }
}
