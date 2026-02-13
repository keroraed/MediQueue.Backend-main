using MediQueue.Core.Enums;

namespace MediQueue.Core.Services;

/// <summary>
/// Service for validating appointment state transitions
/// Follows Single Responsibility Principle
/// </summary>
public interface IAppointmentValidationService
{
    /// <summary>
    /// Validate if status transition is allowed
  /// </summary>
    bool IsValidStatusTransition(AppointmentStatus currentStatus, AppointmentStatus newStatus);
    
    /// <summary>
    /// Get error message for invalid transition
    /// </summary>
    string GetTransitionErrorMessage(AppointmentStatus currentStatus, AppointmentStatus newStatus);
    
    /// <summary>
    /// Validate if clinic owns the appointment
    /// </summary>
    bool ValidateClinicOwnership(int appointmentClinicId, int clinicId);
}
