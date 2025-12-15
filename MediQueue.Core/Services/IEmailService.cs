namespace MediQueue.Core.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendOtpEmailAsync(string toEmail, string otpCode);
    Task SendEmailVerificationOtpAsync(string toEmail, string otpCode, string displayName);
}
