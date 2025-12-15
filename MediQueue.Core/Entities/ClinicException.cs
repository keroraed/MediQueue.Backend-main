namespace MediQueue.Core.Entities;

/// <summary>
/// Clinic Exception - Special closed days
/// </summary>
public class ClinicException
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public DateTime ExceptionDate { get; set; }
    public required string Reason { get; set; }

    // Navigation Properties
    public ClinicProfile Clinic { get; set; } = null!;
}
