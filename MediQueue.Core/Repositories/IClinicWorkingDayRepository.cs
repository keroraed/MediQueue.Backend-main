using MediQueue.Core.Entities;
using MediQueue.Core.Enums;

namespace MediQueue.Core.Repositories;

public interface IClinicWorkingDayRepository : IGenericRepository<ClinicWorkingDay>
{
    Task<IReadOnlyList<ClinicWorkingDay>> GetClinicWorkingDaysAsync(int clinicId);
    Task<ClinicWorkingDay?> GetWorkingDayAsync(int clinicId, DayOfWeekEnum dayOfWeek);
    Task DeleteAllClinicWorkingDaysAsync(int clinicId);
}
