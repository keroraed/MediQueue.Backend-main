using MediQueue.APIs.Errors;
using MediQueue.Core.DTOs;
using MediQueue.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.APIs.Controllers;

public class ClinicsController : BaseApiController
{
    private readonly IClinicService _clinicService;

    public ClinicsController(IClinicService clinicService)
    {
        _clinicService = clinicService;
    }

    /// <summary>
    /// Get clinic profile by ID (Public)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ClinicProfileDto>> GetClinicProfile(int id)
    {
        var clinic = await _clinicService.GetClinicProfileAsync(id);
        
        if (clinic == null)
            return NotFound(new ApiResponse(404, "Clinic not found"));

        return Ok(clinic);
    }

    /// <summary>
    /// Get current clinic's own profile (Clinic only)
    /// </summary>
    [HttpGet("my-profile")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<ClinicProfileDto>> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        var clinic = await _clinicService.GetClinicByUserIdAsync(userId);
      
        if (clinic == null)
            return NotFound(new ApiResponse(404, "Clinic profile not found"));

        return Ok(clinic);
    }

    /// <summary>
    /// Create clinic profile (Clinic only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<ClinicProfileDto>> CreateClinicProfile(CreateClinicProfileDto dto)
{
        try
        {
            var userId = GetCurrentUserId();
            var clinic = await _clinicService.CreateClinicProfileAsync(userId, dto);
         return CreatedAtAction(nameof(GetClinicProfile), new { id = clinic.Id }, clinic);
        }
        catch (InvalidOperationException ex)
   {
       return BadRequest(new ApiResponse(400, ex.Message));
        }
    }

    /// <summary>
    /// Update clinic profile (Clinic only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<ClinicProfileDto>> UpdateClinicProfile(int id, UpdateClinicProfileDto dto)
    {
        try
        {
            var clinic = await _clinicService.UpdateClinicProfileAsync(id, dto);
     return Ok(clinic);
        }
    catch (KeyNotFoundException ex)
        {
      return NotFound(new ApiResponse(404, ex.Message));
     }
    }

    /// <summary>
    /// Get clinic address by clinic ID (Public)
    /// </summary>
    [HttpGet("{id}/address")]
    [AllowAnonymous]
    public async Task<ActionResult<ClinicAddressDto>> GetClinicAddress(int id)
    {
      var address = await _clinicService.GetClinicAddressAsync(id);
   
        if (address == null)
            return NotFound(new ApiResponse(404, "Address not found"));

     return Ok(address);
    }

    /// <summary>
    /// Create or update clinic address (Clinic only)
    /// </summary>
    [HttpPost("{id}/address")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<ClinicAddressDto>> CreateOrUpdateAddress(int id, CreateClinicAddressDto dto)
    {
        try
        {
            var address = await _clinicService.CreateOrUpdateAddressAsync(id, dto);
      return Ok(address);
      }
        catch (KeyNotFoundException ex)
   {
   return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    /// <summary>
    /// Get clinic phones by clinic ID (Public)
    /// </summary>
    [HttpGet("{id}/phones")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ClinicPhoneDto>>> GetClinicPhones(int id)
    {
 var phones = await _clinicService.GetClinicPhonesAsync(id);
        return Ok(phones);
    }

    /// <summary>
    /// Add phone to clinic (Clinic only)
    /// </summary>
    [HttpPost("{id}/phones")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<ClinicPhoneDto>> AddPhone(int id, CreateClinicPhoneDto dto)
    {
    try
        {
      var phone = await _clinicService.AddPhoneAsync(id, dto);
            return CreatedAtAction(nameof(GetClinicPhones), new { id }, phone);
        }
    catch (KeyNotFoundException ex)
        {
return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    /// <summary>
    /// Update clinic phone (Clinic only)
    /// </summary>
    [HttpPut("phones/{phoneId}")]
    [Authorize(Roles = "Clinic")]
    public async Task<ActionResult<ClinicPhoneDto>> UpdatePhone(int phoneId, UpdateClinicPhoneDto dto)
    {
        try
  {
     var phone = await _clinicService.UpdatePhoneAsync(phoneId, dto);
            return Ok(phone);
      }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
        }
  }

    /// <summary>
    /// Delete clinic phone (Clinic only)
    /// </summary>
    [HttpDelete("phones/{phoneId}")]
  [Authorize(Roles = "Clinic")]
    public async Task<ActionResult> DeletePhone(int phoneId)
    {
        try
        {
        await _clinicService.DeletePhoneAsync(phoneId);
     return Ok(new ApiResponse(200, "Phone deleted successfully"));
}
        catch (KeyNotFoundException ex)
        {
     return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    /// <summary>
    /// Search clinics by filters (Public)
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
public async Task<ActionResult<List<ClinicProfileDto>>> SearchClinics(
        [FromQuery] string? specialty,
     [FromQuery] string? city,
   [FromQuery] double? minRating)
    {
        var clinics = await _clinicService.SearchClinicsAsync(specialty, city, minRating);
  return Ok(clinics);
    }

    /// <summary>
    /// Get clinics by specialty (Public)
    /// </summary>
    [HttpGet("specialty/{specialty}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ClinicProfileDto>>> GetClinicsBySpecialty(string specialty)
    {
     var clinics = await _clinicService.GetClinicsBySpecialtyAsync(specialty);
        return Ok(clinics);
    }

    /// <summary>
    /// Get clinics by city (Public)
    /// </summary>
    [HttpGet("city/{city}")]
    [AllowAnonymous]
  public async Task<ActionResult<List<ClinicProfileDto>>> GetClinicsByCity(string city)
    {
        var clinics = await _clinicService.GetClinicsByCityAsync(city);
        return Ok(clinics);
    }
}
