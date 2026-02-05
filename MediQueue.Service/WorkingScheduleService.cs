using MediQueue.Core.DTOs;
using MediQueue.Core.Entities;
using MediQueue.Core.Enums;
using MediQueue.Core.Repositories;
using MediQueue.Core.Services;

namespace MediQueue.Service;

public class WorkingScheduleService : IWorkingScheduleService
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkingScheduleService(IUnitOfWork unitOfWork)
    {
     _unitOfWork = unitOfWork;
    }

    public async Task<List<ClinicWorkingDayDto>> GetWorkingDaysAsync(int clinicId)
    {
  var workingDays = await _unitOfWork.WorkingDays.GetClinicWorkingDaysAsync(clinicId);
        return workingDays.Select(MapToWorkingDayDto).ToList();
    }

    public async Task<ClinicWorkingDayDto?> GetWorkingDayAsync(int clinicId, DayOfWeekEnum dayOfWeek)
    {
        var workingDay = await _unitOfWork.WorkingDays.GetWorkingDayAsync(clinicId, dayOfWeek);
      return workingDay != null ? MapToWorkingDayDto(workingDay) : null;
    }

    public async Task<List<ClinicWorkingDayDto>> BulkUpdateWorkingDaysAsync(int clinicId, BulkUpdateWorkingDaysDto dto)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
        if (clinic == null)
     throw new KeyNotFoundException($"Clinic with ID {clinicId} not found");

        // Delete all existing working days
        await _unitOfWork.WorkingDays.DeleteAllClinicWorkingDaysAsync(clinicId);

        // Add new working days
        foreach (var dayDto in dto.WorkingDays)
        {
         // Only validate working hours if the day is not closed
        if (!dayDto.IsClosed)
          {
          ValidateWorkingHours(dayDto.StartTime, dayDto.EndTime);
    }

            var workingDay = new ClinicWorkingDay
         {
   ClinicId = clinicId,
          DayOfWeek = dayDto.DayOfWeek,
    StartTime = dayDto.StartTime,
     EndTime = dayDto.EndTime,
                IsClosed = dayDto.IsClosed
            };

            _unitOfWork.WorkingDays.Add(workingDay);
        }

    await _unitOfWork.Complete();

        var updatedDays = await _unitOfWork.WorkingDays.GetClinicWorkingDaysAsync(clinicId);
        return updatedDays.Select(MapToWorkingDayDto).ToList();
    }

    public async Task<ClinicWorkingDayDto> UpdateWorkingDayAsync(int workingDayId, UpdateClinicWorkingDayDto dto)
    {
  var workingDay = await _unitOfWork.WorkingDays.GetByIdAsync(workingDayId);
    if (workingDay == null)
            throw new KeyNotFoundException($"Working day with ID {workingDayId} not found");

  // Only validate working hours if the day is not closed
        if (!dto.IsClosed)
     {
            ValidateWorkingHours(dto.StartTime, dto.EndTime);
     }

        workingDay.StartTime = dto.StartTime;
  workingDay.EndTime = dto.EndTime;
        workingDay.IsClosed = dto.IsClosed;

        _unitOfWork.WorkingDays.Update(workingDay);
        await _unitOfWork.Complete();

   return MapToWorkingDayDto(workingDay);
    }

    public async Task<List<ClinicExceptionDto>> GetExceptionsAsync(int clinicId)
    {
        var exceptions = await _unitOfWork.Exceptions.GetClinicExceptionsAsync(clinicId);
     return exceptions.Select(MapToExceptionDto).ToList();
    }

    public async Task<ClinicExceptionDto> AddExceptionAsync(int clinicId, CreateClinicExceptionDto dto)
    {
var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
   if (clinic == null)
     throw new KeyNotFoundException($"Clinic with ID {clinicId} not found");

   // Check if exception already exists for this date
        var existing = await _unitOfWork.Exceptions.GetExceptionByDateAsync(clinicId, dto.ExceptionDate);
        if (existing != null)
     throw new InvalidOperationException($"Exception already exists for date {dto.ExceptionDate:yyyy-MM-dd}");

        var exception = new ClinicException
        {
            ClinicId = clinicId,
  ExceptionDate = dto.ExceptionDate.Date,
            Reason = dto.Reason
        };

   _unitOfWork.Exceptions.Add(exception);
        await _unitOfWork.Complete();

        return MapToExceptionDto(exception);
    }

    public async Task<ClinicExceptionDto> UpdateExceptionAsync(int exceptionId, UpdateClinicExceptionDto dto)
    {
        var exception = await _unitOfWork.Exceptions.GetByIdAsync(exceptionId);
 if (exception == null)
            throw new KeyNotFoundException($"Exception with ID {exceptionId} not found");

        exception.ExceptionDate = dto.ExceptionDate.Date;
        exception.Reason = dto.Reason;

        _unitOfWork.Exceptions.Update(exception);
     await _unitOfWork.Complete();

     return MapToExceptionDto(exception);
    }

    public async Task DeleteExceptionAsync(int exceptionId)
    {
        var exception = await _unitOfWork.Exceptions.GetByIdAsync(exceptionId);
        if (exception == null)
 throw new KeyNotFoundException($"Exception with ID {exceptionId} not found");

        _unitOfWork.Exceptions.Delete(exception);
        await _unitOfWork.Complete();
    }

    public async Task<bool> IsClinicAvailableAsync(int clinicId, DateTime date)
    {
var dayOfWeek = (DayOfWeekEnum)((int)date.DayOfWeek);
      
        // Check if there's an exception for this date
 if (await _unitOfWork.Exceptions.IsExceptionDateAsync(clinicId, date))
return false;

    // Check working day
 var workingDay = await _unitOfWork.WorkingDays.GetWorkingDayAsync(clinicId, dayOfWeek);
        if (workingDay == null || workingDay.IsClosed)
      return false;

        return true;
 }

    public async Task<int> GetDailyCapacityAsync(int clinicId, DateTime date)
    {
        if (!await IsClinicAvailableAsync(clinicId, date))
          return 0;

        var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
    if (clinic == null) return 0;

        var dayOfWeek = (DayOfWeekEnum)((int)date.DayOfWeek);
    var workingDay = await _unitOfWork.WorkingDays.GetWorkingDayAsync(clinicId, dayOfWeek);
        
   if (workingDay == null || workingDay.IsClosed)
            return 0;

        var workingMinutes = (workingDay.EndTime - workingDay.StartTime).TotalMinutes;
        var capacity = (int)(workingMinutes / clinic.SlotDurationMinutes);

        return capacity;
 }

    // Validation helpers
    private void ValidateWorkingHours(TimeSpan startTime, TimeSpan endTime)
 {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

  if (startTime < TimeSpan.Zero || endTime > TimeSpan.FromHours(24))
            throw new ArgumentException("Working hours must be within 24-hour range");
    }

    // Mapping helpers
    private ClinicWorkingDayDto MapToWorkingDayDto(ClinicWorkingDay workingDay)
    {
     return new ClinicWorkingDayDto
        {
            Id = workingDay.Id,
      DayOfWeek = workingDay.DayOfWeek,
            DayName = workingDay.DayOfWeek.ToString(),
  StartTime = workingDay.StartTime,
            EndTime = workingDay.EndTime,
   IsClosed = workingDay.IsClosed
        };
    }

    private ClinicExceptionDto MapToExceptionDto(ClinicException exception)
    {
 return new ClinicExceptionDto
        {
         Id = exception.Id,
  ExceptionDate = exception.ExceptionDate,
         Reason = exception.Reason
        };
    }
}
