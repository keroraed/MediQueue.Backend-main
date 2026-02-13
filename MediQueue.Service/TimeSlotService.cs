using MediQueue.Core.Services;

namespace MediQueue.Service;

/// <summary>
/// Implementation of time slot generation and management
/// Follows Single Responsibility Principle
/// </summary>
public class TimeSlotService : ITimeSlotService
{
    public List<TimeSpan> GenerateTimeSlots(TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes)
    {
        var slots = new List<TimeSpan>();
   var currentTime = startTime;
        var slotDuration = TimeSpan.FromMinutes(slotDurationMinutes);

 while (currentTime.Add(slotDuration) <= endTime)
        {
            slots.Add(currentTime);
  currentTime = currentTime.Add(slotDuration);
     }

  return slots;
    }

    public List<TimeSpan> GetAvailableSlots(List<TimeSpan> allSlots, List<TimeSpan> bookedSlots)
    {
        return allSlots.Except(bookedSlots).ToList();
    }

    public bool IsTimeWithinWorkingHours(TimeSpan time, TimeSpan startTime, TimeSpan endTime)
    {
   return time >= startTime && time < endTime;
  }

    public bool IsValidTimeSlot(TimeSpan time, TimeSpan startTime, int slotDurationMinutes)
    {
        var minutesSinceStart = (time - startTime).TotalMinutes;
    return minutesSinceStart % slotDurationMinutes == 0;
    }
}
