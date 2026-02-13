using MediQueue.Core.Entities;
using MediQueue.Core.Enums;

namespace MediQueue.Core.Repositories;

public interface IAppointmentRepository : IGenericRepository<Appointment>
{
    Task<Appointment?> GetAppointmentWithDetailsAsync(int appointmentId);
    Task<IReadOnlyList<Appointment>> GetClinicAppointmentsByDateAsync(int clinicId, DateTime date);
    Task<IReadOnlyList<Appointment>> GetClinicAppointmentsByDateRangeAsync(int clinicId, DateTime startDate, DateTime endDate); // NEW
    Task<IReadOnlyList<Appointment>> GetAllClinicAppointmentsAsync(int clinicId);
    Task<IReadOnlyList<Appointment>> GetPatientAppointmentsAsync(string patientId);
    Task<IReadOnlyList<Appointment>> GetPatientUpcomingAppointmentsAsync(string patientId);
    Task<IReadOnlyList<Appointment>> GetPatientPastAppointmentsAsync(string patientId);
    Task<int> GetNextQueueNumberAsync(int clinicId, DateTime date);
    Task<int> GetNextQueueNumberWithLockAsync(int clinicId, DateTime date);
    Task<int> GetCurrentQueueNumberAsync(int clinicId, DateTime date);
    Task<int> CountAppointmentsByDateAsync(int clinicId, DateTime date);
    Task<int> CountAppointmentsByStatusAsync(int clinicId, DateTime date, AppointmentStatus status);
    Task<bool> HasCompletedAppointmentAsync(string patientId, int clinicId);
    Task<bool> PatientHasRatedClinicAsync(string patientId, int clinicId);
    Task<bool> IsTimeSlotAvailableAsync(int clinicId, DateTime date, TimeSpan time); // NEW
    Task<List<TimeSpan>> GetBookedTimeSlotsAsync(int clinicId, DateTime date); // NEW
}
