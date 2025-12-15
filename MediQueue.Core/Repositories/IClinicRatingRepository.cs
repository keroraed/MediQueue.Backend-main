using MediQueue.Core.Entities;

namespace MediQueue.Core.Repositories;

public interface IClinicRatingRepository : IGenericRepository<ClinicRating>
{
    Task<IReadOnlyList<ClinicRating>> GetClinicRatingsAsync(int clinicId);
    Task<ClinicRating?> GetPatientRatingForClinicAsync(string patientId, int clinicId);
    Task<double> GetClinicAverageRatingAsync(int clinicId);
    Task<int> GetClinicTotalRatingsAsync(int clinicId);
    Task<Dictionary<int, int>> GetRatingDistributionAsync(int clinicId);
}
