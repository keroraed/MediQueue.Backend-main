using Microsoft.EntityFrameworkCore;
using MediQueue.Core.Entities;
using MediQueue.Core.Repositories;
using MediQueue.Repository.Data;

namespace MediQueue.Repository;

public class ClinicRatingRepository : GenericRepository<ClinicRating>, IClinicRatingRepository
{
    public ClinicRatingRepository(StoreContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ClinicRating>> GetClinicRatingsAsync(int clinicId)
    {
     return await _context.ClinicRatings
   .Where(r => r.ClinicId == clinicId)
            .OrderByDescending(r => r.CreatedAt)
  .ToListAsync();
    }

    public async Task<ClinicRating?> GetPatientRatingForClinicAsync(string patientId, int clinicId)
    {
  return await _context.ClinicRatings
         .FirstOrDefaultAsync(r => r.PatientId == patientId && r.ClinicId == clinicId);
    }

    public async Task<double> GetClinicAverageRatingAsync(int clinicId)
    {
 var ratings = await _context.ClinicRatings
   .Where(r => r.ClinicId == clinicId)
    .Select(r => r.Rating)
     .ToListAsync();

  return ratings.Any() ? ratings.Average() : 0;
    }

    public async Task<int> GetClinicTotalRatingsAsync(int clinicId)
    {
        return await _context.ClinicRatings
     .CountAsync(r => r.ClinicId == clinicId);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(int clinicId)
    {
     var distribution = await _context.ClinicRatings
            .Where(r => r.ClinicId == clinicId)
    .GroupBy(r => r.Rating)
     .Select(g => new { Rating = g.Key, Count = g.Count() })
       .ToListAsync();

      return distribution.ToDictionary(d => d.Rating, d => d.Count);
    }
}
