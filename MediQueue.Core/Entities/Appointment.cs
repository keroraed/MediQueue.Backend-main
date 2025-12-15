using MediQueue.Core.Enums;

namespace MediQueue.Core.Entities;

/// <summary>
/// Appointment - Queue-based booking
/// </summary>
public class Appointment
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public string PatientId { get; set; } = string.Empty; // AppUser.Id (Identity)
    public DateTime AppointmentDate { get; set; }
    public int QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ClinicProfile Clinic { get; set; } = null!;
    // Note: Patient navigation removed as it references AppUser (Identity), not User entity
}
