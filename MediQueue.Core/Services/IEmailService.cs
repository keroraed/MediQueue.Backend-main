namespace MediQueue.Core.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendOtpEmailAsync(string toEmail, string otpCode);
    Task SendEmailVerificationOtpAsync(string toEmail, string otpCode, string displayName);

    /// <summary>
    /// Sent to a patient when their appointment is cancelled by the clinic/doctor.
    /// Informs them their cash will be refunded within 2 working days.
    /// </summary>
    Task SendAppointmentCancellationEmailAsync(
        string toEmail,
        string patientName,
        string doctorName,
        DateTime appointmentDate,
        TimeSpan appointmentTime,
        int queueNumber);

    /// <summary>
    /// Sent to a patient when only 2 patients remain ahead of them in the queue.
    /// </summary>
    Task SendQueueReminderEmailAsync(
        string toEmail,
        string patientName,
        string doctorName,
        DateTime appointmentDate,
        TimeSpan appointmentTime,
        int queueNumber,
        int peopleAhead);
}
