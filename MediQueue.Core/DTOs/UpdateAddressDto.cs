using System.ComponentModel.DataAnnotations;

namespace MediQueue.Core.DTOs;

public class UpdateAddressDto
{
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    [Required]
    public string Street { get; set; }
    
    [Required]
    public string City { get; set; }
    
    [Required]
    public string State { get; set; }
    
    [Required]
    public string ZipCode { get; set; }
    
    [Required]
    public string Country { get; set; }
}
