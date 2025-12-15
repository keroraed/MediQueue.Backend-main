namespace MediQueue.Core.Entities;

/// <summary>
/// Clinic Phone - Multiple phones per clinic
/// </summary>
public class ClinicPhone
{
public int Id { get; set; }
    public int ClinicId { get; set; }
    public required string PhoneNumber { get; set; }

    // Navigation Properties
    public ClinicProfile Clinic { get; set; } = null!;
}
