using MediQueue.Core.DTOs;

namespace MediQueue.Core.Services;

public interface IRatingService
{
    // Rating Management
    Task<ClinicRatingDto> SubmitRatingAsync(string patientId, CreateRatingDto dto);
    Task<ClinicRatingDto> UpdateRatingAsync(int ratingId, UpdateRatingDto dto);
    Task DeleteRatingAsync(int ratingId);
    
    // Queries
    Task<ClinicRatingSummaryDto> GetClinicRatingsAsync(int clinicId, int pageNumber = 1, int pageSize = 10);
    Task<ClinicRatingDto?> GetPatientRatingForClinicAsync(string patientId, int clinicId);
    Task<bool> CanPatientRateClinicAsync(string patientId, int clinicId);
}
