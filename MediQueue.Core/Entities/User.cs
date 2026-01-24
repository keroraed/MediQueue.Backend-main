namespace MediQueue.Core.Entities;

/// <summary>
/// User entity - Represents all users in the system
/// </summary>
public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Role { get; set; } // Admin | Clinic | Patient
    public bool IsVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ClinicProfile? ClinicProfile { get; set; }
    public ICollection<Appointment> PatientAppointments { get; set; } = new List<Appointment>();
    public ICollection<ClinicRating> GivenRatings { get; set; } = new List<ClinicRating>();
}
