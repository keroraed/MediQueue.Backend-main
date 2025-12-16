namespace MediQueue.Core.DTOs;

public class AccountProfileDto
{
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}
