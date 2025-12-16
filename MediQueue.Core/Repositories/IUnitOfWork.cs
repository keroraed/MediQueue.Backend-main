namespace MediQueue.Core.Repositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    
    // Specialized Repositories - Clinic System
    IUserRepository Users { get; }
    IClinicRepository Clinics { get; }
    IAppointmentRepository Appointments { get; }
    IClinicWorkingDayRepository WorkingDays { get; }
    IClinicExceptionRepository Exceptions { get; }
    IClinicRatingRepository Ratings { get; }
    IOtpRepository Otps { get; }
    
    Task<int> Complete();
}
