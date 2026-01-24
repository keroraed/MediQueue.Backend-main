namespace MediQueue.Core.Services;

public interface IOtpService
{
    string GenerateOtp();
    string HashOtp(string otp);
    bool VerifyOtp(string otp, string hashedOtp);
    string GenerateResetToken();
}
