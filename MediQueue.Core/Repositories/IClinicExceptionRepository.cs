using MediQueue.Core.Entities;

namespace MediQueue.Core.Repositories;

public interface IClinicExceptionRepository : IGenericRepository<ClinicException>
{
    Task<IReadOnlyList<ClinicException>> GetClinicExceptionsAsync(int clinicId);
    Task<ClinicException?> GetExceptionByDateAsync(int clinicId, DateTime date);
    Task<bool> IsExceptionDateAsync(int clinicId, DateTime date);
}
