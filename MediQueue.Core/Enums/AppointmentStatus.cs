namespace MediQueue.Core.Enums;

/// <summary>
/// Appointment status enumeration
/// </summary>
public enum AppointmentStatus
{
    /// <summary>
    /// Booked - Appointment is booked and pending
    /// </summary>
    Booked = 1,

    /// <summary>
    /// InProgress - Appointment is currently in progress
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Delayed - Appointment is delayed
    /// </summary>
    Delayed = 3,

 /// <summary>
    /// Canceled - Appointment has been canceled
    /// </summary>
    Canceled = 4,

    /// <summary>
    /// Completed - Appointment has been completed
    /// </summary>
    Completed = 5
}
