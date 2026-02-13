using MediQueue.Core.Entities;

namespace MediQueue.Core.Services;

/// <summary>
/// Service for generating and managing appointment time slots
/// Follows Single Responsibility Principle
/// </summary>
public interface ITimeSlotService
{
    /// <summary>
    /// Generate all possible time slots for a working day
    /// </summary>
    List<TimeSpan> GenerateTimeSlots(TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes);
    
    /// <summary>
    /// Filter available time slots by removing booked ones
 /// </summary>
    List<TimeSpan> GetAvailableSlots(List<TimeSpan> allSlots, List<TimeSpan> bookedSlots);
    
    /// <summary>
 /// Validate if a time falls within working hours
    /// </summary>
    bool IsTimeWithinWorkingHours(TimeSpan time, TimeSpan startTime, TimeSpan endTime);
    
    /// <summary>
    /// Check if time slot aligns with clinic's slot duration
  /// </summary>
  bool IsValidTimeSlot(TimeSpan time, TimeSpan startTime, int slotDurationMinutes);
}
