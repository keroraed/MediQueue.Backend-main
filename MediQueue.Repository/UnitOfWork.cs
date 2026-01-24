using System.Collections;
using MediQueue.Core.Repositories;
using MediQueue.Repository.Data;

namespace MediQueue.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly StoreContext _context;
    private Hashtable? _repositories;

    // Lazy initialization for specialized repositories
    private IUserRepository? _userRepository;
    private IClinicRepository? _clinicRepository;
    private IAppointmentRepository? _appointmentRepository;
    private IClinicWorkingDayRepository? _workingDayRepository;
    private IClinicExceptionRepository? _exceptionRepository;
    private IClinicRatingRepository? _ratingRepository;
    private IOtpRepository? _otpRepository;

    public UnitOfWork(StoreContext context)
    {
        _context = context;
    }

    // Specialized Repositories - Clinic System
    public IUserRepository Users => _userRepository ??= new UserRepository(_context);
    public IClinicRepository Clinics => _clinicRepository ??= new ClinicRepository(_context);
    public IAppointmentRepository Appointments => _appointmentRepository ??= new AppointmentRepository(_context);
    public IClinicWorkingDayRepository WorkingDays => _workingDayRepository ??= new ClinicWorkingDayRepository(_context);
    public IClinicExceptionRepository Exceptions => _exceptionRepository ??= new ClinicExceptionRepository(_context);
    public IClinicRatingRepository Ratings => _ratingRepository ??= new ClinicRatingRepository(_context);
    
    // Note: Otps repository requires separate handling (uses Identity context)
    public IOtpRepository Otps => throw new NotImplementedException("Use IOtpRepository directly with dependency injection");

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        if (_repositories == null) _repositories = new Hashtable();

        var type = typeof(TEntity).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(GenericRepository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context);

            _repositories.Add(type, repositoryInstance);
        }

        return (IGenericRepository<TEntity>)_repositories[type]!;
    }

    public async Task<int> Complete()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
