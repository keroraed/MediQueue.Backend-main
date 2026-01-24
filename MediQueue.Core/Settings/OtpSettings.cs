namespace MediQueue.Core.Settings;

/// <summary>
/// Configuration settings for OTP functionality
/// </summary>
public class OtpSettings
{
    public int ExpirationMinutes { get; set; } = 10;
    public int MaxFailedAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 30;
    public int ResendCooldownSeconds { get; set; } = 60;
}
