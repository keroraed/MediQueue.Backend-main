using MediQueue.Core.Enums;
using System;

namespace MediQueue.Core.Entities;

/// <summary>
/// Appointment - Queue-based booking with fixed time slots
/// </summary>
public class Appointment
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public string PatientId { get; set; } = string.Empty; // AppUser.Id (Identity)
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; } // NEW: Fixed time slot (e.g., 10:30)
    public int QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ClinicProfile Clinic { get; set; } = null!;
    // Note: Patient navigation removed as it references AppUser (Identity), not User entity
}
