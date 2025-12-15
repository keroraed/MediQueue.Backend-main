namespace MediQueue.Core.Entities;

/// <summary>
/// Clinic Rating - Patient reviews
/// </summary>
public class ClinicRating
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public string PatientId { get; set; } = string.Empty; // AppUser.Id (Identity)
    public int Rating { get; set; } // 1-5
    public string? Review { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ClinicProfile Clinic { get; set; } = null!;
    // Note: Patient navigation removed as it references AppUser (Identity), not User entity
}
