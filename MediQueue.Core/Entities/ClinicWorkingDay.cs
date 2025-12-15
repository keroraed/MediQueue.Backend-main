using MediQueue.Core.Enums;

namespace MediQueue.Core.Entities;

/// <summary>
/// Clinic Working Day - Weekly schedule
/// </summary>
public class ClinicWorkingDay
{
 public int Id { get; set; }
    public int ClinicId { get; set; }
 public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsClosed { get; set; } = false;

    // Navigation Properties
    public ClinicProfile Clinic { get; set; } = null!;
}
