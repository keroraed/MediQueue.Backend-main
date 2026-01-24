using Microsoft.EntityFrameworkCore;
using MediQueue.Core.Entities;
using MediQueue.Core.Enums;
using MediQueue.Core.Repositories;
using MediQueue.Repository.Data;

namespace MediQueue.Repository;

public class ClinicWorkingDayRepository : GenericRepository<ClinicWorkingDay>, IClinicWorkingDayRepository
{
    public ClinicWorkingDayRepository(StoreContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ClinicWorkingDay>> GetClinicWorkingDaysAsync(int clinicId)
    {
        return await _context.ClinicWorkingDays
       .Where(w => w.ClinicId == clinicId)
.OrderBy(w => w.DayOfWeek)
 .ToListAsync();
    }

    public async Task<ClinicWorkingDay?> GetWorkingDayAsync(int clinicId, DayOfWeekEnum dayOfWeek)
    {
        return await _context.ClinicWorkingDays
.FirstOrDefaultAsync(w => w.ClinicId == clinicId && w.DayOfWeek == dayOfWeek);
    }

    public async Task DeleteAllClinicWorkingDaysAsync(int clinicId)
    {
  var workingDays = await _context.ClinicWorkingDays
    .Where(w => w.ClinicId == clinicId)
    .ToListAsync();
    
  _context.ClinicWorkingDays.RemoveRange(workingDays);
    }
}
