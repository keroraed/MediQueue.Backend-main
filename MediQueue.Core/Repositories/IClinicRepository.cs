using MediQueue.Core.Entities;

namespace MediQueue.Core.Repositories;

public interface IClinicRepository : IGenericRepository<ClinicProfile>
{
    Task<ClinicProfile?> GetClinicByUserIdAsync(string appUserId);
    Task<ClinicProfile?> GetClinicWithDetailsAsync(int clinicId);
    Task<IReadOnlyList<ClinicProfile>> GetClinicsBySpecialtyAsync(string specialty);
    Task<IReadOnlyList<ClinicProfile>> GetClinicsByCityAsync(string city);
  Task<IReadOnlyList<ClinicProfile>> SearchClinicsAsync(string? specialty, string? city, double? minRating);
    Task<bool> ClinicExistsForUserAsync(string appUserId);
}
