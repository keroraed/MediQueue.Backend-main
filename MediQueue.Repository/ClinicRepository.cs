using Microsoft.EntityFrameworkCore;
using MediQueue.Core.Entities;
using MediQueue.Core.Repositories;
using MediQueue.Repository.Data;

namespace MediQueue.Repository;

public class ClinicRepository : GenericRepository<ClinicProfile>, IClinicRepository
{
    public ClinicRepository(StoreContext context) : base(context)
    {
    }

    public async Task<ClinicProfile?> GetClinicByUserIdAsync(string appUserId)
    {
        return await _context.ClinicProfiles
       .Include(c => c.Address)
.Include(c => c.Phones)
     .Include(c => c.WorkingDays)
     .Include(c => c.Ratings)
   .FirstOrDefaultAsync(c => c.AppUserId == appUserId);
    }

    public async Task<ClinicProfile?> GetClinicWithDetailsAsync(int clinicId)
    {
 return await _context.ClinicProfiles
   .Include(c => c.Address)
      .Include(c => c.Phones)
          .Include(c => c.WorkingDays)
            .Include(c => c.Ratings)
     .FirstOrDefaultAsync(c => c.Id == clinicId);
    }

    public async Task<IReadOnlyList<ClinicProfile>> GetClinicsBySpecialtyAsync(string specialty)
    {
  return await _context.ClinicProfiles
   .Include(c => c.Address)
    .Include(c => c.Phones)
       .Include(c => c.Ratings)
       .Where(c => c.Specialty.ToLower().Contains(specialty.ToLower()))
  .ToListAsync();
    }

    public async Task<IReadOnlyList<ClinicProfile>> GetClinicsByCityAsync(string city)
    {
     return await _context.ClinicProfiles
.Include(c => c.Address)
   .Include(c => c.Phones)
  .Include(c => c.Ratings)
            .Where(c => c.Address != null && c.Address.City.ToLower().Contains(city.ToLower()))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ClinicProfile>> SearchClinicsAsync(string? specialty, string? city, double? minRating)
{
        var query = _context.ClinicProfiles
 .Include(c => c.Address)
            .Include(c => c.Phones)
    .Include(c => c.Ratings)
         .AsQueryable();

        if (!string.IsNullOrEmpty(specialty))
        {
query = query.Where(c => c.Specialty.ToLower().Contains(specialty.ToLower()));
        }

  if (!string.IsNullOrEmpty(city))
 {
            query = query.Where(c => c.Address != null && c.Address.City.ToLower().Contains(city.ToLower()));
 }

        if (minRating.HasValue)
        {
       query = query.Where(c => c.Ratings.Any() && c.Ratings.Average(r => r.Rating) >= minRating.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<bool> ClinicExistsForUserAsync(string appUserId)
    {
        return await _context.ClinicProfiles.AnyAsync(c => c.AppUserId == appUserId);
    }
}
