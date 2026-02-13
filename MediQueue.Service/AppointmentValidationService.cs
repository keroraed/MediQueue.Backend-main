using MediQueue.Core.Enums;
using MediQueue.Core.Services;

namespace MediQueue.Service;

/// <summary>
/// Implementation of appointment validation logic
/// Follows Single Responsibility Principle
/// </summary>
public class AppointmentValidationService : IAppointmentValidationService
{
    public bool IsValidStatusTransition(AppointmentStatus currentStatus, AppointmentStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
    // From Booked
            (AppointmentStatus.Booked, AppointmentStatus.InProgress) => true,
        (AppointmentStatus.Booked, AppointmentStatus.Canceled) => true,
    (AppointmentStatus.Booked, AppointmentStatus.Delayed) => true,
            
            // From InProgress
            (AppointmentStatus.InProgress, AppointmentStatus.Completed) => true,
        (AppointmentStatus.InProgress, AppointmentStatus.Delayed) => true,
     
            // From Delayed
          (AppointmentStatus.Delayed, AppointmentStatus.InProgress) => true,
(AppointmentStatus.Delayed, AppointmentStatus.Canceled) => true,
            
            // Same status (no change)
            _ when currentStatus == newStatus => true,
  
            // All other transitions are invalid
            _ => false
      };
    }

    public string GetTransitionErrorMessage(AppointmentStatus currentStatus, AppointmentStatus newStatus)
    {
        if (currentStatus == newStatus)
    return $"Appointment is already in {currentStatus} status";

        return currentStatus switch
        {
      AppointmentStatus.Completed => "Cannot change status of completed appointments",
            AppointmentStatus.Canceled => "Cannot change status of canceled appointments",
     _ => $"Cannot change appointment status from {currentStatus} to {newStatus}"
        };
    }

    public bool ValidateClinicOwnership(int appointmentClinicId, int clinicId)
    {
        return appointmentClinicId == clinicId;
    }
}
