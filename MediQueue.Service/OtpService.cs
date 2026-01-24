using System.Security.Cryptography;
using System.Text;
using MediQueue.Core.Services;

namespace MediQueue.Service;

public class OtpService : IOtpService
{
    public string GenerateOtp()
    {
        using var rng = RandomNumberGenerator.Create();
        var otpBytes = new byte[4];
        rng.GetBytes(otpBytes);
        var otp = BitConverter.ToUInt32(otpBytes, 0) % 1000000;
        return otp.ToString("D6");
    }

    public string HashOtp(string otp)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyOtp(string otp, string hashedOtp)
    {
        var otpHash = HashOtp(otp);
        return otpHash == hashedOtp;
    }

    public string GenerateResetToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[32];
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }
}
