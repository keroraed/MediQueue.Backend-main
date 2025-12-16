using MediQueue.Core.DTOs;
using MediQueue.Core.Enums;

namespace MediQueue.Core.Services;

public interface IAppointmentService
{
    // Booking
    Task<AppointmentDto> BookAppointmentAsync(string patientId, BookAppointmentDto dto);
    Task CancelAppointmentAsync(int appointmentId, string userId);
    
    // Appointment Management (Clinic)
  Task<AppointmentDto> UpdateAppointmentStatusAsync(int appointmentId, UpdateAppointmentStatusDto dto);
    Task<AppointmentDto> CallNextPatientAsync(int clinicId, DateTime date);
    Task<AppointmentDto> MarkInProgressAsync(int appointmentId);
    Task<AppointmentDto> MarkCompletedAsync(int appointmentId);
    Task<AppointmentDto> MarkDelayedAsync(int appointmentId);
    
    // Queries
    Task<AppointmentDetailsDto?> GetAppointmentDetailsAsync(int appointmentId);
    Task<ClinicQueueDto> GetClinicQueueAsync(int clinicId, DateTime date);
    Task<PatientAppointmentHistoryDto> GetPatientHistoryAsync(string patientId);
    Task<List<AppointmentDto>> GetPatientUpcomingAppointmentsAsync(string patientId);
    Task<List<AppointmentDto>> GetPatientPastAppointmentsAsync(string patientId);
  
    // Statistics
    Task<int> GetEstimatedWaitTimeAsync(int appointmentId);
}
