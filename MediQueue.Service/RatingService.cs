using MediQueue.Core.DTOs;
using MediQueue.Core.Entities;
using MediQueue.Core.Repositories;
using MediQueue.Core.Services;

namespace MediQueue.Service;

public class RatingService : IRatingService
{
  private readonly IUnitOfWork _unitOfWork;

    public RatingService(IUnitOfWork unitOfWork)
    {
    _unitOfWork = unitOfWork;
    }

    public async Task<ClinicRatingDto> SubmitRatingAsync(string patientId, CreateRatingDto dto)
    {
        // Validate rating range
     if (dto.Rating < 1 || dto.Rating > 5)
  throw new ArgumentException("Rating must be between 1 and 5");

   // Check if clinic exists
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(dto.ClinicId);
if (clinic == null)
      throw new KeyNotFoundException($"Clinic with ID {dto.ClinicId} not found");

    // Check if patient can rate this clinic
        if (!await CanPatientRateClinicAsync(patientId, dto.ClinicId))
    throw new InvalidOperationException("You can only rate clinics where you have completed appointments");

        // Check if patient already rated this clinic
        var existingRating = await _unitOfWork.Ratings.GetPatientRatingForClinicAsync(patientId, dto.ClinicId);
        if (existingRating != null)
      throw new InvalidOperationException("You have already rated this clinic. Use update instead.");

 var rating = new ClinicRating
 {
    ClinicId = dto.ClinicId,
         PatientId = patientId,
      Rating = dto.Rating,
  Review = dto.Review
        };

  _unitOfWork.Ratings.Add(rating);
      await _unitOfWork.Complete();

      return MapToRatingDto(rating, "Patient");
    }

    public async Task<ClinicRatingDto> UpdateRatingAsync(int ratingId, UpdateRatingDto dto)
    {
        var rating = await _unitOfWork.Ratings.GetByIdAsync(ratingId);
        if (rating == null)
    throw new KeyNotFoundException($"Rating with ID {ratingId} not found");

        // Validate rating range
  if (dto.Rating < 1 || dto.Rating > 5)
       throw new ArgumentException("Rating must be between 1 and 5");

        rating.Rating = dto.Rating;
        rating.Review = dto.Review;

  _unitOfWork.Ratings.Update(rating);
        await _unitOfWork.Complete();

      return MapToRatingDto(rating, "Patient");
 }

    public async Task DeleteRatingAsync(int ratingId)
    {
        var rating = await _unitOfWork.Ratings.GetByIdAsync(ratingId);
if (rating == null)
  throw new KeyNotFoundException($"Rating with ID {ratingId} not found");

        _unitOfWork.Ratings.Delete(rating);
   await _unitOfWork.Complete();
    }

    public async Task<ClinicRatingSummaryDto> GetClinicRatingsAsync(int clinicId, int pageNumber = 1, int pageSize = 10)
    {
var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
  if (clinic == null)
     throw new KeyNotFoundException($"Clinic with ID {clinicId} not found");

  var allRatings = await _unitOfWork.Ratings.GetClinicRatingsAsync(clinicId);
  var distribution = await _unitOfWork.Ratings.GetRatingDistributionAsync(clinicId);
   var averageRating = await _unitOfWork.Ratings.GetClinicAverageRatingAsync(clinicId);

      // Paginate ratings
     var paginatedRatings = allRatings
      .Skip((pageNumber - 1) * pageSize)
.Take(pageSize)
.ToList();

        return new ClinicRatingSummaryDto
        {
      ClinicId = clinicId,
     ClinicName = clinic.DoctorName,
      AverageRating = Math.Round(averageRating, 2),
         TotalRatings = allRatings.Count,
  FiveStarCount = distribution.GetValueOrDefault(5, 0),
         FourStarCount = distribution.GetValueOrDefault(4, 0),
            ThreeStarCount = distribution.GetValueOrDefault(3, 0),
     TwoStarCount = distribution.GetValueOrDefault(2, 0),
          OneStarCount = distribution.GetValueOrDefault(1, 0),
          RecentRatings = paginatedRatings.Select(r => MapToRatingDto(r, "Patient")).ToList()
   };
    }

 public async Task<ClinicRatingDto?> GetPatientRatingForClinicAsync(string patientId, int clinicId)
    {
        var rating = await _unitOfWork.Ratings.GetPatientRatingForClinicAsync(patientId, clinicId);
        if (rating == null) return null;

   return MapToRatingDto(rating, "Patient");
    }

    public async Task<bool> CanPatientRateClinicAsync(string patientId, int clinicId)
    {
 // Patient can rate if they have at least one completed appointment
   return await _unitOfWork.Appointments.HasCompletedAppointmentAsync(patientId, clinicId);
    }

    // Mapping helpers
    private ClinicRatingDto MapToRatingDto(ClinicRating rating, string patientName)
    {
        return new ClinicRatingDto
     {
  Id = rating.Id,
  ClinicId = rating.ClinicId,
     PatientId = rating.PatientId,
       PatientName = patientName,
    Rating = rating.Rating,
    Review = rating.Review,
   CreatedAt = rating.CreatedAt
      };
    }
}
