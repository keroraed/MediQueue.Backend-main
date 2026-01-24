namespace MediQueue.Core.DTOs;

public class UserDTO
{
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string Role { get; set; } // Single role: "Admin", "Clinic", or "Patient"
}
