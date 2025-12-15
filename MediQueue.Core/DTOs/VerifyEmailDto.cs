using System.ComponentModel.DataAnnotations;

namespace MediQueue.Core.DTOs;

public class VerifyEmailDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string OtpCode { get; set; }
}
