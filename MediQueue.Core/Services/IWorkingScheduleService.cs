using MediQueue.Core.DTOs;
using MediQueue.Core.Enums;

namespace MediQueue.Core.Services;

public interface IWorkingScheduleService
{
    // Working Days
    Task<List<ClinicWorkingDayDto>> GetWorkingDaysAsync(int clinicId);
    Task<ClinicWorkingDayDto?> GetWorkingDayAsync(int clinicId, DayOfWeekEnum dayOfWeek);
    Task<List<ClinicWorkingDayDto>> BulkUpdateWorkingDaysAsync(int clinicId, BulkUpdateWorkingDaysDto dto);
    Task<ClinicWorkingDayDto> UpdateWorkingDayAsync(int workingDayId, UpdateClinicWorkingDayDto dto);
    
    // Exceptions
    Task<List<ClinicExceptionDto>> GetExceptionsAsync(int clinicId);
    Task<ClinicExceptionDto> AddExceptionAsync(int clinicId, CreateClinicExceptionDto dto);
    Task<ClinicExceptionDto> UpdateExceptionAsync(int exceptionId, UpdateClinicExceptionDto dto);
    Task DeleteExceptionAsync(int exceptionId);
    
    // Availability Check
    Task<bool> IsClinicAvailableAsync(int clinicId, DateTime date);
    Task<int> GetDailyCapacityAsync(int clinicId, DateTime date);
}
