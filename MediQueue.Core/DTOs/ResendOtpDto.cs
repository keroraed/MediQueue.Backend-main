using System.ComponentModel.DataAnnotations;

namespace MediQueue.Core.DTOs;

public class ResendOtpDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
