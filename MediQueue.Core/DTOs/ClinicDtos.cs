using System.ComponentModel.DataAnnotations;

namespace MediQueue.Core.DTOs;

// ==================== Clinic Profile DTOs ====================

public class ClinicProfileDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public required string DoctorName { get; set; }
    public required string Specialty { get; set; }
    public string? Description { get; set; }
    public int SlotDurationMinutes { get; set; }
public ClinicAddressDto? Address { get; set; }
    public List<ClinicPhoneDto> Phones { get; set; } = new();
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateClinicProfileDto
{
    [Required]
    [StringLength(200)]
  public required string DoctorName { get; set; }

    [Required]
    [StringLength(200)]
    public required string Specialty { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(10, 120)]
    public int SlotDurationMinutes { get; set; } = 30;
}

public class UpdateClinicProfileDto
{
    [Required]
    [StringLength(200)]
    public required string DoctorName { get; set; }

    [Required]
    [StringLength(200)]
    public required string Specialty { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(10, 120)]
    public int SlotDurationMinutes { get; set; }
}

// ==================== Clinic Address DTOs ====================

public class ClinicAddressDto
{
    public int Id { get; set; }
  public required string Country { get; set; }
    public required string City { get; set; }
public required string Area { get; set; }
    public required string Street { get; set; }
    public required string Building { get; set; }
    public string? Notes { get; set; }
}

public class CreateClinicAddressDto
{
    [Required]
    [StringLength(100)]
    public required string Country { get; set; }

    [Required]
    [StringLength(100)]
    public required string City { get; set; }

    [Required]
    [StringLength(100)]
    public required string Area { get; set; }

    [Required]
    [StringLength(200)]
    public required string Street { get; set; }

    [Required]
    [StringLength(50)]
    public required string Building { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class UpdateClinicAddressDto
{
    [Required]
    [StringLength(100)]
 public required string Country { get; set; }

    [Required]
    [StringLength(100)]
    public required string City { get; set; }

    [Required]
    [StringLength(100)]
    public required string Area { get; set; }

    [Required]
    [StringLength(200)]
    public required string Street { get; set; }

  [Required]
    [StringLength(50)]
    public required string Building { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

// ==================== Clinic Phone DTOs ====================

public class ClinicPhoneDto
{
    public int Id { get; set; }
    public required string PhoneNumber { get; set; }
}

public class CreateClinicPhoneDto
{
    [Required]
    [Phone]
    [StringLength(20)]
    public required string PhoneNumber { get; set; }
}

public class UpdateClinicPhoneDto
{
    [Required]
    [Phone]
    [StringLength(20)]
  public required string PhoneNumber { get; set; }
}
