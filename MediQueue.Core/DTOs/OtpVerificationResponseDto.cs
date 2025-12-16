namespace MediQueue.Core.DTOs;

public class OtpVerificationResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string ResetToken { get; set; }
}
