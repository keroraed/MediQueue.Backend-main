using Microsoft.EntityFrameworkCore;
using MediQueue.Core.Entities;
using MediQueue.Core.Enums;
using MediQueue.Core.Repositories;
using MediQueue.Repository.Data;

namespace MediQueue.Repository;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(StoreContext context) : base(context)
    {
    }

    public async Task<Appointment?> GetAppointmentWithDetailsAsync(int appointmentId)
    {
      return await _context.Appointments
     .Include(a => a.Clinic)
  .Include(a => a.Clinic.Address)
    .Include(a => a.Clinic.Phones)
      .FirstOrDefaultAsync(a => a.Id == appointmentId);
    }

    public async Task<IReadOnlyList<Appointment>> GetClinicAppointmentsByDateAsync(int clinicId, DateTime date)
    {
   var dateOnly = date.Date;
        return await _context.Appointments
        .Include(a => a.Clinic)
      .Where(a => a.ClinicId == clinicId && a.AppointmentDate.Date == dateOnly)
        .OrderBy(a => a.QueueNumber)
      .ToListAsync();
    }

    // ? NEW: Get all clinic appointments (no date filter)
  public async Task<IReadOnlyList<Appointment>> GetAllClinicAppointmentsAsync(int clinicId)
    {
        return await _context.Appointments
     .Include(a => a.Clinic)
            .Where(a => a.ClinicId == clinicId)
            .OrderByDescending(a => a.AppointmentDate)
        .ThenBy(a => a.QueueNumber)
  .ToListAsync();
    }

    public async Task<IReadOnlyList<Appointment>> GetPatientAppointmentsAsync(string patientId)
    {
  return await _context.Appointments
   .Include(a => a.Clinic)
  .Include(a => a.Clinic.Address)
     .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate)
     .ToListAsync();
    }

    public async Task<IReadOnlyList<Appointment>> GetPatientUpcomingAppointmentsAsync(string patientId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Appointments
 .Include(a => a.Clinic)
     .Include(a => a.Clinic.Address)
 .Where(a => a.PatientId == patientId 
              && a.AppointmentDate >= today
          && a.Status != AppointmentStatus.Canceled
    && a.Status != AppointmentStatus.Completed)
       .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.QueueNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Appointment>> GetPatientPastAppointmentsAsync(string patientId)
    {
  var today = DateTime.UtcNow.Date;
        return await _context.Appointments
            .Include(a => a.Clinic)
            .Include(a => a.Clinic.Address)
   .Where(a => a.PatientId == patientId 
      && (a.AppointmentDate < today
        || a.Status == AppointmentStatus.Canceled
        || a.Status == AppointmentStatus.Completed))
     .OrderByDescending(a => a.AppointmentDate)
     .ToListAsync();
    }

    public async Task<int> GetNextQueueNumberAsync(int clinicId, DateTime date)
    {
        var dateOnly = date.Date;
        var maxQueueNumber = await _context.Appointments
            .Where(a => a.ClinicId == clinicId && a.AppointmentDate.Date == dateOnly)
            .MaxAsync(a => (int?)a.QueueNumber) ?? 0;
   
        return maxQueueNumber + 1;
    }

    public async Task<int> GetNextQueueNumberWithLockAsync(int clinicId, DateTime date)
    {
        var dateOnly = date.Date;
        
        // Use raw SQL with row-level locking to prevent race conditions
        // UPDLOCK + ROWLOCK ensures exclusive access during queue number generation
        var maxQueueNumber = await _context.Appointments
            .FromSqlRaw(@"
                SELECT * FROM Appointments WITH (UPDLOCK, ROWLOCK)
                WHERE ClinicId = {0} AND CAST(AppointmentDate AS DATE) = CAST({1} AS DATE)
            ", clinicId, dateOnly)
            .MaxAsync(a => (int?)a.QueueNumber) ?? 0;
        
        return maxQueueNumber + 1;
    }

    public async Task<int> GetCurrentQueueNumberAsync(int clinicId, DateTime date)
    {
        var dateOnly = date.Date;
        var current = await _context.Appointments
          .Where(a => a.ClinicId == clinicId 
    && a.AppointmentDate.Date == dateOnly
         && (a.Status == AppointmentStatus.InProgress || a.Status == AppointmentStatus.Completed))
          .MaxAsync(a => (int?)a.QueueNumber) ?? 0;
    
 return current;
    }

    public async Task<int> CountAppointmentsByDateAsync(int clinicId, DateTime date)
    {
        var dateOnly = date.Date;
  return await _context.Appointments
    .CountAsync(a => a.ClinicId == clinicId && a.AppointmentDate.Date == dateOnly);
    }

    public async Task<int> CountAppointmentsByStatusAsync(int clinicId, DateTime date, AppointmentStatus status)
    {
   var dateOnly = date.Date;
        return await _context.Appointments
   .CountAsync(a => a.ClinicId == clinicId 
       && a.AppointmentDate.Date == dateOnly
   && a.Status == status);
    }

  public async Task<bool> HasCompletedAppointmentAsync(string patientId, int clinicId)
    {
        return await _context.Appointments
            .AnyAsync(a => a.PatientId == patientId 
                && a.ClinicId == clinicId
                && a.Status == AppointmentStatus.Completed);
    }

    public async Task<bool> PatientHasRatedClinicAsync(string patientId, int clinicId)
    {
        return await _context.ClinicRatings
    .AnyAsync(r => r.PatientId == patientId && r.ClinicId == clinicId);
    }
}
