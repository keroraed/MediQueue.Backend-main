using MediQueue.Core.Enums;

namespace MediQueue.Core.Entities.Identity;

public class Otp
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Code { get; set; }
    public OtpPurpose Purpose { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpirationDate { get; set; }
    public bool IsUsed { get; set; } = false;
    public int FailedAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiration { get; set; }
}
