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
    public DateTime AppointmentDate { get; set; }
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
 public DateTime AppointmentDate { get; set; }
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
