using System.ComponentModel.DataAnnotations;

namespace MediQueue.Core.DTOs;

// ==================== Rating DTOs ====================

public class ClinicRatingDto
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public required string PatientName { get; set; }
    public int Rating { get; set; }
    public string? Review { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateRatingDto
{
    [Required]
    public int ClinicId { get; set; }

 [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Review { get; set; }
}

public class UpdateRatingDto
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Review { get; set; }
}

public class ClinicRatingSummaryDto
{
    public int ClinicId { get; set; }
public required string ClinicName { get; set; }
    public double AverageRating { get; set; }
  public int TotalRatings { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
 public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }
    public List<ClinicRatingDto> RecentRatings { get; set; } = new();
}
