using Microsoft.AspNetCore.Identity;

namespace MediQueue.Core.Entities.Identity;

public class AppUser : IdentityUser
{
    public required string DisplayName { get; set; }
    public List<Address> Addresses { get; set; } = new List<Address>();
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
    public string? FcmDeviceToken { get; set; }
    
    // Patient-specific fields
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; } // Male, Female, Other
    public string? BloodType { get; set; } // A+, A-, B+, B-, AB+, AB-, O+, O-
    public string? EmergencyContact { get; set; }
    public string? EmergencyContactPhone { get; set; }
}
