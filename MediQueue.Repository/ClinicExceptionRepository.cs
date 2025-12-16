using Microsoft.EntityFrameworkCore;
using MediQueue.Core.Entities;
using MediQueue.Core.Repositories;
using MediQueue.Repository.Data;

namespace MediQueue.Repository;

public class ClinicExceptionRepository : GenericRepository<ClinicException>, IClinicExceptionRepository
{
    public ClinicExceptionRepository(StoreContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ClinicException>> GetClinicExceptionsAsync(int clinicId)
    {
     return await _context.ClinicExceptions
     .Where(e => e.ClinicId == clinicId)
   .OrderBy(e => e.ExceptionDate)
  .ToListAsync();
    }

    public async Task<ClinicException?> GetExceptionByDateAsync(int clinicId, DateTime date)
    {
        var dateOnly = date.Date;
        return await _context.ClinicExceptions
      .FirstOrDefaultAsync(e => e.ClinicId == clinicId && e.ExceptionDate.Date == dateOnly);
}

    public async Task<bool> IsExceptionDateAsync(int clinicId, DateTime date)
    {
 var dateOnly = date.Date;
        return await _context.ClinicExceptions
            .AnyAsync(e => e.ClinicId == clinicId && e.ExceptionDate.Date == dateOnly);
    }
}
