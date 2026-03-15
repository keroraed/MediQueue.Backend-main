using MediQueue.Core.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace MediQueue.Service;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(
            _configuration["EmailSettings:SenderName"],
            _configuration["EmailSettings:SenderEmail"]));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _configuration["EmailSettings:SmtpServer"],
            int.Parse(_configuration["EmailSettings:Port"]),
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _configuration["EmailSettings:Username"],
            _configuration["EmailSettings:Password"]);

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public async Task SendOtpEmailAsync(string toEmail, string otpCode)
    {
    var subject = "MediQueue â€” Password reset code";
        var body = $@"
    <html>
    <head>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    </head>
    <body style='margin: 0; padding: 0; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif;'>
        <div style='max-width: 600px; margin: 0 auto; padding: 40px 20px;'>
            <!-- Main Card -->
            <div style='background: white; border-radius: 20px; box-shadow: 0 20px 60px rgba(0,0,0,0.3); overflow: hidden;'>
                
                <!-- Header Section -->
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center;'>
                    <div style='background: rgba(255,255,255,0.2); width: 80px; height: 80px; margin: 0 auto 20px; border-radius: 50%; display: flex; align-items: center; justify-content: center; backdrop-filter: blur(10px);'>
                    </div>
                    <h1 style='color: white; margin: 0; font-size: 28px; font-weight: 700; text-shadow: 0 2px 4px rgba(0,0,0,0.1);'>Security Verification</h1>
                    <p style='color: rgba(255,255,255,0.9); margin: 10px 0 0; font-size: 16px;'>MediQueue</p>
                </div>
                
                <!-- Content Section -->
                <div style='padding: 40px 30px;'>
                    <p style='color: #333; font-size: 16px; line-height: 1.6; margin: 0 0 25px;'>
                        Hello,
                    </p>
                    <p style='color: #666; font-size: 15px; line-height: 1.6; margin: 0 0 30px;'>
                        We received a request to reset your password. Use the verification code below to complete the password reset for your MediQueue account:
                    </p>
                    
                    <!-- OTP Code Box -->
                    <div style='background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%); border-radius: 16px; padding: 30px; text-align: center; margin: 30px 0; border: 3px dashed #667eea; position: relative;'>
                        <div style='color: #999; font-size: 12px; text-transform: uppercase; letter-spacing: 2px; margin-bottom: 10px; font-weight: 600;'>Your Verification Code</div>
                        <div style='font-size: 42px; font-weight: 800; letter-spacing: 12px; color: #667eea; text-shadow: 2px 2px 4px rgba(0,0,0,0.1); font-family: ""Courier New"", monospace;'>
                            {otpCode}
                        </div>
                        <div style='margin-top: 15px; display: inline-block;'>
                            <div style='background: #fff; border-radius: 20px; padding: 8px 20px; display: inline-flex; align-items: center; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                                <svg width='16' height='16' viewBox='0 0 24 24' fill='none' style='margin-right: 8px;'>
                                    <circle cx='12' cy='12' r='10' stroke='#f59e0b' stroke-width='2'/>
                                    <path d='M12 6V12L16 14' stroke='#f59e0b' stroke-width='2' stroke-linecap='round'/>
                                </svg>
                                <span style='color: #f59e0b; font-size: 13px; font-weight: 600;'>Expires in 10 minutes</span>
                            </div>
                        </div>
                    </div>
                    
                    <!-- Warning Box -->
                    <div style='background: #fef3c7; border-left: 4px solid #f59e0b; border-radius: 8px; padding: 15px 20px; margin: 25px 0;'>
                        <p style='margin: 0; color: #92400e; font-size: 14px; line-height: 1.5;'>
                            <strong>?? Security Notice:</strong> If you didn't request this password reset, please ignore this email and ensure your account is secure.
                        </p>
                    </div>
                    
                    <!-- Tips Section -->
                    <div style='background: #f8fafc; border-radius: 12px; padding: 20px; margin-top: 30px;'>
                        <p style='margin: 0 0 10px; color: #64748b; font-size: 13px; font-weight: 600; text-transform: uppercase; letter-spacing: 1px;'>?? Security Tips</p>
                        <ul style='margin: 0; padding-left: 20px; color: #64748b; font-size: 14px; line-height: 1.8;'>
                            <li>Never share your OTP code with anyone.</li>
                                <li>MediQueue will never ask for your code via phone or direct message.</li>
                        </ul>
                    </div>
                </div>
                
                <!-- Footer -->
                <div style='background: #f8fafc; padding: 25px 30px; border-top: 1px solid #e2e8f0;'>
                        <p style='margin: 0; color: #94a3b8; font-size: 13px; text-align: center; line-height: 1.6;'>
                            This is an automated message from <strong style='color: #667eea;'>MediQueue</strong>. Please do not reply to this email.
                        </p>
                    <div style='text-align: center; margin-top: 15px; padding-top: 15px; border-top: 1px solid #e2e8f0;'>
                        <p style='margin: 0; color: #cbd5e1; font-size: 12px;'>
                                Â© 2024 MediQueue. All rights reserved.
                        </p>
                    </div>
                </div>
                
            </div>
            
            <!-- Bottom Spacer -->
            <div style='height: 40px;'></div>
            
        </div>
    </body>
    </html>
";

		await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendEmailVerificationOtpAsync(string toEmail, string otpCode, string displayName)
    {
        var subject = "Verify your email â€” MediQueue";
		var body = $@"
    <html>
    <head>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    </head>
    <body style='margin: 0; padding: 0; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif;'>
        <div style='max-width: 600px; margin: 0 auto; padding: 40px 20px;'>
            <!-- Main Card -->
            <div style='background: white; border-radius: 20px; box-shadow: 0 20px 60px rgba(0,0,0,0.3); overflow: hidden;'>
                
                <!-- Header Section -->
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center;'>
                    <div style='background: rgba(255,255,255,0.2); width: 80px; height: 80px; margin: 0 auto 20px; border-radius: 50%; display: inline-flex; align-items: center; justify-content: center; backdrop-filter: blur(10px); font-size: 40px;'>
                        ??
                    </div>
                    <h1 style='color: white; margin: 0; font-size: 28px; font-weight: 700; text-shadow: 0 2px 4px rgba(0,0,0,0.1);'>Welcome to MediQueue!</h1>
                    <p style='color: rgba(255,255,255,0.9); margin: 10px 0 0; font-size: 16px;'>We're excited to have you on board</p>
                </div>
                
                <!-- Content Section -->
                <div style='padding: 40px 30px;'>
                    <p style='color: #333; font-size: 18px; line-height: 1.6; margin: 0 0 10px; font-weight: 600;'>
                        Hi {displayName},
                    </p>
                    <p style='color: #666; font-size: 15px; line-height: 1.6; margin: 0 0 30px;'>
                        Thank you for joining MediQueue! You're just one step away from getting started. Please verify your email address using the code below:
                    </p>
                    
                    <!-- OTP Code Box -->
                    <div style='background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%); border-radius: 16px; padding: 30px; text-align: center; margin: 30px 0; border: 3px dashed #667eea; position: relative;'>
                        <div style='color: #999; font-size: 12px; text-transform: uppercase; letter-spacing: 2px; margin-bottom: 10px; font-weight: 600;'>Your Verification Code</div>
                        <div style='font-size: 42px; font-weight: 800; letter-spacing: 12px; color: #667eea; text-shadow: 2px 2px 4px rgba(0,0,0,0.1); font-family: ""Courier New"", monospace;'>
                            {otpCode}
                        </div>
                        <div style='margin-top: 15px; display: inline-block;'>
                            <div style='background: #fff; border-radius: 20px; padding: 8px 20px; display: inline-flex; align-items: center; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                                <span style='margin-right: 8px; font-size: 16px;'>?</span>
                                <span style='color: #f59e0b; font-size: 13px; font-weight: 600;'>Expires in 10 minutes</span>
                            </div>
                        </div>
                    </div>
                    
                    <!-- Info Box -->
                    <div style='background: #f0f9ff; border-left: 4px solid #3b82f6; border-radius: 8px; padding: 15px 20px; margin: 25px 0;'>
                        <p style='margin: 0; color: #1e40af; font-size: 14px; line-height: 1.5;'>
                            <strong>?? Note:</strong> If you didn't create an account with Resouq, you can safely ignore this email.
                        </p>
                    </div>
                    
                    <!-- What's Next Section -->
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 12px; padding: 25px; margin-top: 30px; color: white;'>
                        <p style='margin: 0 0 15px; font-size: 16px; font-weight: 600;'>?? What's Next?</p>
                        <ul style='margin: 0; padding-left: 20px; font-size: 14px; line-height: 1.8;'>
                            <li>Verify your email with the code above</li>
                            <li>Complete your profile setup</li>
                            <li>Start exploring MediQueue's features</li>
                        </ul>
                    </div>
                    
                    <!-- Support Section -->
                    <div style='text-align: center; margin-top: 30px; padding: 20px; background: #f8fafc; border-radius: 12px;'>
                        <p style='margin: 0 0 10px; color: #64748b; font-size: 14px;'>
                            Need help getting started?
                        </p>
                        <p style='margin: 0; color: #667eea; font-size: 14px; font-weight: 600;'>
                            Contact our support team
                        </p>
                    </div>
                </div>
                
                <!-- Footer -->
                <div style='background: #f8fafc; padding: 25px 30px; border-top: 1px solid #e2e8f0;'>
                    <p style='margin: 0; color: #94a3b8; font-size: 13px; text-align: center; line-height: 1.6;'>
                        This is an automated message from <strong style='color: #667eea;'>MediQueue</strong>. Please do not reply to this email.
                    </p>
                    <div style='text-align: center; margin-top: 15px; padding-top: 15px; border-top: 1px solid #e2e8f0;'>
                        <p style='margin: 0; color: #cbd5e1; font-size: 12px;'>
                            Â© 2024 MediQueue. All rights reserved.
                        </p>
                    </div>
                </div>
                
            </div>
            
            <!-- Bottom Spacer -->
            <div style='height: 40px;'></div>
            
        </div>
    </body>
    </html>
";

		await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendAppointmentCancellationEmailAsync(
        string toEmail,
        string patientName,
        string doctorName,
        DateTime appointmentDate,
        TimeSpan appointmentTime,
        int queueNumber)
    {
        var subject = "Appointment Cancelled – MediQueue";
        var formattedDate = appointmentDate.ToString("dddd, MMMM dd, yyyy");
        var formattedTime = DateTime.Today.Add(appointmentTime).ToString("hh:mm tt");

        var body = $@"
<html>
<head><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
<body style='margin:0;padding:0;background:linear-gradient(135deg,#f5f7fa 0%,#c3cfe2 100%);font-family:-apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,Arial,sans-serif;'>
  <div style='max-width:600px;margin:0 auto;padding:40px 20px;'>
    <div style='background:white;border-radius:20px;box-shadow:0 20px 60px rgba(0,0,0,0.15);overflow:hidden;'>

      <!-- Header -->
      <div style='background:linear-gradient(135deg,#ef4444 0%,#dc2626 100%);padding:40px 30px;text-align:center;'>
        <h1 style='color:white;margin:0;font-size:26px;font-weight:700;'>Appointment Cancelled</h1>
        <p style='color:rgba(255,255,255,0.9);margin:10px 0 0;font-size:15px;'>MediQueue</p>
      </div>

      <!-- Body -->
      <div style='padding:40px 30px;'>
        <p style='color:#333;font-size:16px;margin:0 0 10px;font-weight:600;'>Dear {patientName},</p>
        <p style='color:#555;font-size:15px;line-height:1.7;margin:0 0 25px;'>
          We're sorry to inform you that <strong>Dr. {doctorName}</strong> has cancelled your upcoming appointment.
          Your payment will be <strong>refunded to your account within 2 working days</strong>.
        </p>

        <!-- Appointment Details -->
        <div style='background:#fef2f2;border:1px solid #fecaca;border-radius:12px;padding:25px;margin:25px 0;'>
          <p style='margin:0 0 12px;color:#991b1b;font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:1px;'>Cancelled Appointment</p>
          <table style='width:100%;border-collapse:collapse;'>
            <tr><td style='color:#555;padding:6px 0;font-size:14px;width:40%;'>Doctor</td><td style='color:#111;font-weight:600;font-size:14px;'>Dr. {doctorName}</td></tr>
            <tr><td style='color:#555;padding:6px 0;font-size:14px;'>Date</td><td style='color:#111;font-weight:600;font-size:14px;'>{formattedDate}</td></tr>
            <tr><td style='color:#555;padding:6px 0;font-size:14px;'>Time</td><td style='color:#111;font-weight:600;font-size:14px;'>{formattedTime}</td></tr>
            <tr><td style='color:#555;padding:6px 0;font-size:14px;'>Queue #</td><td style='color:#111;font-weight:600;font-size:14px;'>#{queueNumber}</td></tr>
          </table>
        </div>

        <!-- Refund Notice -->
        <div style='background:#f0fdf4;border-left:4px solid #22c55e;border-radius:8px;padding:15px 20px;margin:25px 0;'>
          <p style='margin:0;color:#166534;font-size:14px;line-height:1.6;'>
            <strong>Refund Notice:</strong> Any payment made for this appointment will be automatically refunded to your account within <strong>2 working days</strong>.
            If you have any questions, please contact our support team.
          </p>
        </div>

        <p style='color:#555;font-size:14px;line-height:1.7;'>
          We apologise for any inconvenience. You can book a new appointment at any time through the MediQueue app.
        </p>
      </div>

      <!-- Footer -->
      <div style='background:#f8fafc;padding:20px 30px;border-top:1px solid #e2e8f0;text-align:center;'>
        <p style='margin:0;color:#94a3b8;font-size:13px;'>This is an automated message from <strong style='color:#667eea;'>MediQueue</strong>. Please do not reply to this email.</p>
        <p style='margin:8px 0 0;color:#cbd5e1;font-size:12px;'>© 2024 MediQueue. All rights reserved.</p>
      </div>

    </div>
  </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendQueueReminderEmailAsync(
        string toEmail,
        string patientName,
        string doctorName,
        DateTime appointmentDate,
        TimeSpan appointmentTime,
        int queueNumber,
        int peopleAhead)
    {
        var subject = "You're Almost Up! – MediQueue Queue Reminder";
        var formattedDate = appointmentDate.ToString("dddd, MMMM dd, yyyy");
        var formattedTime = DateTime.Today.Add(appointmentTime).ToString("hh:mm tt");

        var body = $@"
<html>
<head><meta name='viewport' content='width=device-width, initial-scale=1.0'></head>
<body style='margin:0;padding:0;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);font-family:-apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,Arial,sans-serif;'>
  <div style='max-width:600px;margin:0 auto;padding:40px 20px;'>
    <div style='background:white;border-radius:20px;box-shadow:0 20px 60px rgba(0,0,0,0.3);overflow:hidden;'>

      <!-- Header -->
      <div style='background:linear-gradient(135deg,#f59e0b 0%,#d97706 100%);padding:40px 30px;text-align:center;'>
        <div style='font-size:48px;margin-bottom:10px;'>⏰</div>
        <h1 style='color:white;margin:0;font-size:26px;font-weight:700;'>Almost Your Turn!</h1>
        <p style='color:rgba(255,255,255,0.9);margin:10px 0 0;font-size:15px;'>MediQueue Queue Reminder</p>
      </div>

      <!-- Body -->
      <div style='padding:40px 30px;'>
        <p style='color:#333;font-size:16px;margin:0 0 10px;font-weight:600;'>Dear {patientName},</p>
        <p style='color:#555;font-size:15px;line-height:1.7;margin:0 0 25px;'>
          Heads up! There are only <strong>{peopleAhead} patient{(peopleAhead == 1 ? "" : "s")}</strong> ahead of you in the queue with
          <strong>Dr. {doctorName}</strong>. Please make sure you are ready and close by.
        </p>

        <!-- Queue Status -->
        <div style='background:linear-gradient(135deg,#fef3c7 0%,#fde68a 100%);border:1px solid #fcd34d;border-radius:12px;padding:25px;margin:25px 0;text-align:center;'>
          <p style='margin:0 0 5px;color:#92400e;font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:1px;'>Your Queue Position</p>
          <div style='font-size:52px;font-weight:800;color:#d97706;margin:10px 0;'>#{queueNumber}</div>
          <p style='margin:0;color:#92400e;font-size:15px;font-weight:600;'>Only {peopleAhead} ahead of you!</p>
        </div>

        <!-- Appointment Details -->
        <div style='background:#f8fafc;border-radius:12px;padding:20px;margin:20px 0;'>
          <p style='margin:0 0 12px;color:#475569;font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:1px;'>Appointment Details</p>
          <table style='width:100%;border-collapse:collapse;'>
            <tr><td style='color:#555;padding:5px 0;font-size:14px;width:40%;'>Doctor</td><td style='color:#111;font-weight:600;font-size:14px;'>Dr. {doctorName}</td></tr>
            <tr><td style='color:#555;padding:5px 0;font-size:14px;'>Date</td><td style='color:#111;font-weight:600;font-size:14px;'>{formattedDate}</td></tr>
            <tr><td style='color:#555;padding:5px 0;font-size:14px;'>Time</td><td style='color:#111;font-weight:600;font-size:14px;'>{formattedTime}</td></tr>
          </table>
        </div>

        <!-- Action Note -->
        <div style='background:#eff6ff;border-left:4px solid #3b82f6;border-radius:8px;padding:15px 20px;'>
          <p style='margin:0;color:#1e40af;font-size:14px;line-height:1.6;'>
            <strong>Please be ready:</strong> Head to the clinic now so you don't miss your turn. Arriving late may result in losing your spot in the queue.
          </p>
        </div>
      </div>

      <!-- Footer -->
      <div style='background:#f8fafc;padding:20px 30px;border-top:1px solid #e2e8f0;text-align:center;'>
        <p style='margin:0;color:#94a3b8;font-size:13px;'>This is an automated message from <strong style='color:#667eea;'>MediQueue</strong>. Please do not reply to this email.</p>
        <p style='margin:8px 0 0;color:#cbd5e1;font-size:12px;'>© 2024 MediQueue. All rights reserved.</p>
      </div>

    </div>
  </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body);
    }
}
