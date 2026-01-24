using MediQueue.APIs.Errors;
using MediQueue.Core.DTOs;
using MediQueue.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.APIs.Controllers;

public class RatingsController : BaseApiController
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
    _ratingService = ratingService;
    }

    /// <summary>
    /// Submit a rating for a clinic (Patient only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<ClinicRatingDto>> SubmitRating(CreateRatingDto dto)
    {
        try
{
      var patientId = GetCurrentUserId();
   var rating = await _ratingService.SubmitRatingAsync(patientId, dto);
 return CreatedAtAction(nameof(GetClinicRatings), new { clinicId = dto.ClinicId }, rating);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
    return BadRequest(new ApiResponse(400, ex.Message));
        }
  catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse(400, ex.Message));
        }
    }

    /// <summary>
    /// Update an existing rating (Patient only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Patient")]
public async Task<ActionResult<ClinicRatingDto>> UpdateRating(int id, UpdateRatingDto dto)
    {
        try
     {
            var rating = await _ratingService.UpdateRatingAsync(id, dto);
      return Ok(rating);
        }
        catch (KeyNotFoundException ex)
        {
  return NotFound(new ApiResponse(404, ex.Message));
 }
  catch (ArgumentException ex)
   {
      return BadRequest(new ApiResponse(400, ex.Message));
        }
    }

    /// <summary>
    /// Delete a rating (Patient only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult> DeleteRating(int id)
    {
        try
        {
            await _ratingService.DeleteRatingAsync(id);
            return Ok(new ApiResponse(200, "Rating deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
   return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    /// <summary>
    /// Get all ratings for a clinic with summary (Public)
    /// </summary>
    [HttpGet("clinic/{clinicId}")]
    public async Task<ActionResult<ClinicRatingSummaryDto>> GetClinicRatings(
        int clinicId,
      [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
       var summary = await _ratingService.GetClinicRatingsAsync(clinicId, pageNumber, pageSize);
            return Ok(summary);
     }
        catch (KeyNotFoundException ex)
        {
  return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    /// <summary>
    /// Get patient's rating for a specific clinic (Patient only)
    /// </summary>
    [HttpGet("clinic/{clinicId}/my-rating")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<ClinicRatingDto>> GetMyRatingForClinic(int clinicId)
    {
     var patientId = GetCurrentUserId();
        var rating = await _ratingService.GetPatientRatingForClinicAsync(patientId, clinicId);

        if (rating == null)
  return NotFound(new ApiResponse(404, "You have not rated this clinic yet"));

        return Ok(rating);
    }

    /// <summary>
  /// Check if patient can rate a clinic (Patient only)
    /// </summary>
    [HttpGet("clinic/{clinicId}/can-rate")]
    [Authorize(Roles = "Patient")]
    public async Task<ActionResult<bool>> CanRateClinic(int clinicId)
    {
        var patientId = GetCurrentUserId();
        var canRate = await _ratingService.CanPatientRateClinicAsync(patientId, clinicId);
        return Ok(new { canRate });
    }
}
