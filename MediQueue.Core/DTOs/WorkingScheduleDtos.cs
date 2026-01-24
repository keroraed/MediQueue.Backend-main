using System.ComponentModel.DataAnnotations;
using MediQueue.Core.Enums;

namespace MediQueue.Core.DTOs;

// ==================== Working Day DTOs ====================

public class ClinicWorkingDayDto
{
    public int Id { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public required string DayName { get; set; }
  public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsClosed { get; set; }
}

public class CreateClinicWorkingDayDto
{
    [Required]
    [Range(0, 6)]
 public DayOfWeekEnum DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public bool IsClosed { get; set; } = false;
}

public class UpdateClinicWorkingDayDto
{
    [Required]
  public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public bool IsClosed { get; set; }
}

public class BulkUpdateWorkingDaysDto
{
    [Required]
    public List<CreateClinicWorkingDayDto> WorkingDays { get; set; } = new();
}

// ==================== Clinic Exception DTOs ====================

public class ClinicExceptionDto
{
    public int Id { get; set; }
    public DateTime ExceptionDate { get; set; }
    public required string Reason { get; set; }
}

public class CreateClinicExceptionDto
{
    [Required]
    public DateTime ExceptionDate { get; set; }

    [Required]
    [StringLength(500)]
    public required string Reason { get; set; }
}

public class UpdateClinicExceptionDto
{
    [Required]
    public DateTime ExceptionDate { get; set; }

    [Required]
    [StringLength(500)]
    public required string Reason { get; set; }
}
