using System.ComponentModel.DataAnnotations;

namespace MediQueue.Core.DTOs;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
