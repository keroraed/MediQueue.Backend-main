namespace MediQueue.Core.Entities;

/// <summary>
/// Clinic Address - One address per clinic
/// </summary>
public class ClinicAddress
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public required string Country { get; set; }
    public required string City { get; set; }
    public required string Area { get; set; }
    public required string Street { get; set; }
    public required string Building { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public ClinicProfile Clinic { get; set; } = null!;
}
