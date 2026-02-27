using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MediQueue.Core.DTOs;

/// <summary>
/// DTO used by Admin to create a clinic account from the admin dashboard.
/// Contains identity fields, clinic profile fields, address, and optional profile picture.
/// </summary>
public class AdminCreateClinicDto
{
    // ── Identity fields ──
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

    // ── Clinic profile fields ──
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

    // ── Address fields ──
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

    // ── Additional phone numbers (optional) ──
    public List<string>? AdditionalPhones { get; set; }

    // ── Consultation fee ──
    [Range(0, 100000)]
    public decimal? ConsultationFee { get; set; }

    // ── Payment methods (one or more: VodafoneCash, InstaPay, Visa) ──
    public List<string>? PaymentMethods { get; set; }

    // ── Profile picture (optional) ──
    /// <summary>Doctor profile picture uploaded by the admin.</summary>
    public IFormFile? ProfilePicture { get; set; }
}
