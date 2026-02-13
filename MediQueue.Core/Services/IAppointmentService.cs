using MediQueue.Core.DTOs;
using MediQueue.Core.Enums;

namespace MediQueue.Core.Services;

public interface IAppointmentService
{
    // Booking - AUTO-QUEUE SYSTEM
    Task<AppointmentDto> BookAppointmentAsync(string patientId, BookAppointmentDto dto);
    Task CancelAppointmentAsync(int appointmentId, string userId);
    
    // Get next available booking date
    Task<NextAvailableDateDto> GetNextAvailableDateAsync(int clinicId, DateTime fromDate);
    
    // Appointment Management (Clinic)
    Task<AppointmentDto> UpdateAppointmentStatusAsync(int appointmentId, string clinicUserId, UpdateAppointmentStatusDto dto);
    Task<AppointmentDto> CallNextPatientAsync(int clinicId, DateTime date);
    Task<AppointmentDto> MarkInProgressAsync(int appointmentId, string clinicUserId);
    Task<AppointmentDto> MarkCompletedAsync(int appointmentId, string clinicUserId);
    Task<AppointmentDto> MarkDelayedAsync(int appointmentId, string clinicUserId);
    
    // Queries
    Task<AppointmentDetailsDto?> GetAppointmentDetailsAsync(int appointmentId);
    Task<ClinicQueueDto> GetClinicQueueAsync(int clinicId, DateTime date);
    Task<WeeklyQueueDto> GetClinicWeeklyQueueAsync(int clinicId, DateTime startDate);
    Task<PatientAppointmentHistoryDto> GetPatientHistoryAsync(string patientId);
    Task<List<AppointmentDto>> GetPatientUpcomingAppointmentsAsync(string patientId);
    Task<List<AppointmentDto>> GetPatientPastAppointmentsAsync(string patientId);
    
    // Statistics
    Task<int> GetEstimatedWaitTimeAsync(int appointmentId);
}
