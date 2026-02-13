using System.ComponentModel.DataAnnotations;

namespace MediQueue.Core.DTOs;

/// <summary>
/// Base registration DTO
/// </summary>
public class RegisterDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    [MinLength(3)]
    public string DisplayName { get; set; } = null!;
  
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = null!;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = null!;

    [Required]
    public string Role { get; set; } = null!; // "Patient" or "Clinic"
}

/// <summary>
/// Patient-specific registration DTO
/// </summary>
public class RegisterPatientDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    [MinLength(3)]
    public string DisplayName { get; set; } = null!;
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = null!;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = null!;

    // Required patient-specific fields
    [Required]
    public DateTime DateOfBirth { get; set; }
    
    [Required]
    public string Gender { get; set; } = null!; // Male, Female, Other
    
    [Required]
    public string BloodType { get; set; } = null!; // A+, A-, B+, B-, AB+, AB-, O+, O-
  
    // Optional fields
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyContactPhone { get; set; }
}

/// <summary>
/// Clinic-specific registration DTO
/// </summary>
public class RegisterClinicDTO
{
    // Identity fields
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    [MinLength(3)]
    public string DisplayName { get; set; } = null!; // Will be used as DoctorName
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = null!;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = null!;

    // Clinic-specific fields
    [Required]
    [MinLength(3)]
    public string DoctorName { get; set; } = null!;

    [Required]
    [MinLength(3)]
    public string Specialty { get; set; } = null!;

    [Required]
    [MinLength(10)]
    [StringLength(2000)]
    public string Description { get; set; } = null!;

    [Range(5, 120)]
    public int SlotDurationMinutes { get; set; } = 30;

    // Address fields
    [Required]
    public string Country { get; set; } = null!;

    [Required]
    public string City { get; set; } = null!;

    [Required]
    public string Area { get; set; } = null!;

    [Required]
    public string Street { get; set; } = null!;

    [Required]
    public string Building { get; set; } = null!;

    public string? AddressNotes { get; set; }

    // Additional phone numbers (optional)
    public List<string>? AdditionalPhones { get; set; }
}
