using MediQueue.Core.DTOs;
using MediQueue.Core.Entities;
using MediQueue.Core.Entities.Identity;
using MediQueue.Core.Enums;
using MediQueue.Core.Repositories;
using MediQueue.Core.Services;
using Microsoft.AspNetCore.Identity;

namespace MediQueue.Service;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkingScheduleService _scheduleService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITimeSlotService _timeSlotService;
    private readonly IAppointmentValidationService _validationService;
    private readonly IClinicService _clinicService;

    public AppointmentService(
        IUnitOfWork unitOfWork,
        IWorkingScheduleService scheduleService,
        UserManager<AppUser> userManager,
        ITimeSlotService timeSlotService,
        IAppointmentValidationService validationService,
        IClinicService clinicService)
    {
        _unitOfWork = unitOfWork;
      _scheduleService = scheduleService;
        _userManager = userManager;
        _timeSlotService = timeSlotService;
 _validationService = validationService;
     _clinicService = clinicService;
    }

    public async Task<AppointmentDto> BookAppointmentAsync(string patientId, BookAppointmentDto dto)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(dto.ClinicId);
   if (clinic == null)
       throw new KeyNotFoundException($"Clinic with ID {dto.ClinicId} not found");

        // Validate date is in the future
        if (dto.AppointmentDate.Date < DateTime.UtcNow.Date)
            throw new InvalidOperationException("Cannot book appointments in the past");

        // Check if clinic is open on this day of week
        var dayOfWeek = (DayOfWeekEnum)((int)dto.AppointmentDate.DayOfWeek);
        var workingDay = await _unitOfWork.WorkingDays.GetWorkingDayAsync(dto.ClinicId, dayOfWeek);

        if (workingDay == null || workingDay.IsClosed)
            throw new InvalidOperationException($"Clinic is not open on {dayOfWeek}s");

        // Check for exceptions/closures on this specific date
   if (await _unitOfWork.Exceptions.IsExceptionDateAsync(dto.ClinicId, dto.AppointmentDate.Date))
 {
      var exception = await _unitOfWork.Exceptions.GetExceptionByDateAsync(dto.ClinicId, dto.AppointmentDate.Date);
 throw new InvalidOperationException($"Clinic is closed on {dto.AppointmentDate:yyyy-MM-dd}. Reason: {exception?.Reason ?? "Holiday/Closure"}");
        }

        // AUTO-QUEUE: Generate all possible time slots using clinic's SlotDurationMinutes
 var allSlots = _timeSlotService.GenerateTimeSlots(
        workingDay.StartTime,
      workingDay.EndTime,
            clinic.SlotDurationMinutes);

     // Get already booked time slots
        var bookedSlots = await _unitOfWork.Appointments.GetBookedTimeSlotsAsync(dto.ClinicId, dto.AppointmentDate);

   // Find first available slot (AUTO-ASSIGNMENT)
        var nextAvailableSlot = allSlots.FirstOrDefault(slot => !bookedSlots.Contains(slot));

        if (nextAvailableSlot == default(TimeSpan))
     {
       // Clinic is fully booked - get next available date
   var nextAvailable = await GetNextAvailableDateAsync(dto.ClinicId, dto.AppointmentDate.AddDays(1));
    
       var message = $"Clinic is fully booked on {dto.AppointmentDate:yyyy-MM-dd}";
            if (nextAvailable.Date.HasValue)
            {
                message += $". Next available date: {nextAvailable.Date.Value:yyyy-MM-dd} ({nextAvailable.DayName})";
     }
          else
 {
       message += ". No available dates found in the next 30 days. Please contact the clinic directly";
            }
            
    throw new InvalidOperationException(message);
        }

        // Get next queue number with row-level locking to prevent race conditions
        var queueNumber = await _unitOfWork.Appointments.GetNextQueueNumberWithLockAsync(dto.ClinicId, dto.AppointmentDate);

        // Get patient information from Identity
    var patient = await _userManager.FindByIdAsync(patientId);

        var appointment = new Appointment
        {
            ClinicId = dto.ClinicId,
        PatientId = patientId,
 AppointmentDate = dto.AppointmentDate.Date,
            AppointmentTime = nextAvailableSlot, // AUTO-ASSIGNED based on clinic's SlotDurationMinutes!
            QueueNumber = queueNumber,
      Status = AppointmentStatus.Booked
     };

_unitOfWork.Appointments.Add(appointment);
        await _unitOfWork.Complete();

        var created = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(appointment.Id);
  return MapToAppointmentDto(created!, patient);
  }

    // REMOVED: GetAvailableTimeSlotsAsync - Not needed in auto-queue system

    public async Task<NextAvailableDateDto> GetNextAvailableDateAsync(int clinicId, DateTime fromDate)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
     if (clinic == null)
throw new KeyNotFoundException($"Clinic with ID {clinicId} not found");

        var currentDate = fromDate.Date;
        var maxDaysToCheck = 30; // Look up to 30 days ahead
     
      for (int i = 0; i < maxDaysToCheck; i++)
        {
     var checkDate = currentDate.AddDays(i);
            
            // Check if clinic is available (no exceptions, is working day)
          if (!await _scheduleService.IsClinicAvailableAsync(clinicId, checkDate))
  continue;

     // Check if there's capacity
       var capacity = await _scheduleService.GetDailyCapacityAsync(clinicId, checkDate);
          var currentBookings = await _unitOfWork.Appointments.CountAppointmentsByDateAsync(clinicId, checkDate);

      if (currentBookings < capacity)
            {
        // Found an available date
        var dayOfWeek = (DayOfWeekEnum)((int)checkDate.DayOfWeek);
           var workingDay = await _unitOfWork.WorkingDays.GetWorkingDayAsync(clinicId, dayOfWeek);

      return new NextAvailableDateDto
  {
          Date = checkDate,
            DayName = checkDate.ToString("dddd"),
       StartTime = workingDay?.StartTime,
         EndTime = workingDay?.EndTime,
          AvailableSlots = capacity - currentBookings,
        Message = $"Next available date: {checkDate:dddd, MMMM dd, yyyy}"
     };
            }
   }

  // No available dates found in the next 30 days
     return new NextAvailableDateDto
        {
       Date = null,
      DayName = null,
      StartTime = null,
         EndTime = null,
        AvailableSlots = 0,
          Message = "No available dates found in the next 30 days. Please contact the clinic directly"
  };
    }

    public async Task CancelAppointmentAsync(int appointmentId, string userId)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
        if (appointment == null)
            throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");

        // Check authorization - user can only cancel their own appointments
        if (appointment.PatientId != userId)
          throw new UnauthorizedAccessException("You can only cancel your own appointments");

        // Check if appointment can be canceled
      if (appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed appointments");

        if (appointment.Status == AppointmentStatus.Canceled)
            throw new InvalidOperationException("Appointment is already canceled");

        appointment.Status = AppointmentStatus.Canceled;
        _unitOfWork.Appointments.Update(appointment);
    await _unitOfWork.Complete();
    }

    public async Task<AppointmentDto> UpdateAppointmentStatusAsync(int appointmentId, string clinicUserId, UpdateAppointmentStatusDto dto)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
if (appointment == null)
            throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");

        // Get clinic profile for authorization check
        var clinic = await _clinicService.GetClinicByUserIdAsync(clinicUserId);
        if (clinic == null)
      throw new InvalidOperationException("Clinic profile not found for this user");

        // Validate clinic ownership
   if (!_validationService.ValidateClinicOwnership(appointment.ClinicId, clinic.Id))
            throw new UnauthorizedAccessException("You can only update appointments for your own clinic");

        // Validate status transition
   if (!_validationService.IsValidStatusTransition(appointment.Status, dto.Status))
        {
       var errorMessage = _validationService.GetTransitionErrorMessage(appointment.Status, dto.Status);
         throw new InvalidOperationException(errorMessage);
      }

        appointment.Status = dto.Status;
        _unitOfWork.Appointments.Update(appointment);
   await _unitOfWork.Complete();

        var updated = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(appointmentId);
        var patient = await _userManager.FindByIdAsync(appointment.PatientId);
        return MapToAppointmentDto(updated!, patient);
    }

    public async Task<AppointmentDto> CallNextPatientAsync(int clinicId, DateTime date)
    {
        var appointments = await _unitOfWork.Appointments.GetClinicAppointmentsByDateAsync(clinicId, date);
        
        var nextAppointment = appointments
  .Where(a => a.Status == AppointmentStatus.Booked)
        .OrderBy(a => a.AppointmentTime)
            .ThenBy(a => a.QueueNumber)
       .FirstOrDefault();

        if (nextAppointment == null)
  throw new InvalidOperationException("No booked appointments found");

        nextAppointment.Status = AppointmentStatus.InProgress;
        _unitOfWork.Appointments.Update(nextAppointment);
        await _unitOfWork.Complete();

  var updated = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(nextAppointment.Id);
        var patient = await _userManager.FindByIdAsync(nextAppointment.PatientId);
        return MapToAppointmentDto(updated!, patient);
    }

    public async Task<AppointmentDto> MarkInProgressAsync(int appointmentId, string clinicUserId)
    {
        return await UpdateAppointmentStatusAsync(appointmentId, clinicUserId, new UpdateAppointmentStatusDto 
    { 
Status = AppointmentStatus.InProgress 
   });
  }

    public async Task<AppointmentDto> MarkCompletedAsync(int appointmentId, string clinicUserId)
    {
    return await UpdateAppointmentStatusAsync(appointmentId, clinicUserId, new UpdateAppointmentStatusDto 
        { 
            Status = AppointmentStatus.Completed 
        });
    }

    public async Task<AppointmentDto> MarkDelayedAsync(int appointmentId, string clinicUserId)
    {
        return await UpdateAppointmentStatusAsync(appointmentId, clinicUserId, new UpdateAppointmentStatusDto 
        { 
            Status = AppointmentStatus.Delayed 
        });
    }

    public async Task<AppointmentDetailsDto?> GetAppointmentDetailsAsync(int appointmentId)
    {
        var appointment = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(appointmentId);
      if (appointment == null) return null;

        var patient = await _userManager.FindByIdAsync(appointment.PatientId);
        var currentQueueNumber = await _unitOfWork.Appointments.GetCurrentQueueNumberAsync(
      appointment.ClinicId, appointment.AppointmentDate);

        var peopleAhead = appointment.QueueNumber - currentQueueNumber;
        if (peopleAhead < 0) peopleAhead = 0;

      var estimatedWait = await GetEstimatedWaitTimeAsync(appointmentId);

        return new AppointmentDetailsDto
        {
            Id = appointment.Id,
   ClinicId = appointment.ClinicId,
          ClinicName = appointment.Clinic.DoctorName, // Using DoctorName as clinic display name
            DoctorName = appointment.Clinic.DoctorName,
 Specialty = appointment.Clinic.Specialty,
         Address = appointment.Clinic.Address != null ? MapToAddressDto(appointment.Clinic.Address) : null,
  Phones = appointment.Clinic.Phones.Select(MapToPhoneDto).ToList(),
  PatientId = appointment.PatientId,
PatientName = patient?.DisplayName,
         PatientPhone = patient?.PhoneNumber,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentTime = appointment.AppointmentTime,
            QueueNumber = appointment.QueueNumber,
            Status = appointment.Status,
   StatusName = appointment.Status.ToString(),
            EstimatedWaitMinutes = estimatedWait,
CurrentQueueNumber = currentQueueNumber,
    PeopleAhead = peopleAhead,
 CreatedAt = appointment.CreatedAt
        };
    }

    public async Task<ClinicQueueDto> GetClinicQueueAsync(int clinicId, DateTime date)
  {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
        if (clinic == null)
            throw new KeyNotFoundException($"Clinic with ID {clinicId} not found");

      var appointments = await _unitOfWork.Appointments.GetClinicAppointmentsByDateAsync(clinicId, date);
 var currentQueueNumber = await _unitOfWork.Appointments.GetCurrentQueueNumberAsync(clinicId, date);

  // Fetch patient information for all appointments
 var appointmentDtos = new List<AppointmentDto>();
        foreach (var apt in appointments)
        {
            var patient = await _userManager.FindByIdAsync(apt.PatientId);
     appointmentDtos.Add(MapToAppointmentDto(apt, patient));
     }

        return new ClinicQueueDto
  {
          ClinicId = clinicId,
    ClinicName = clinic.DoctorName,
   DoctorName = clinic.DoctorName,
            Date = date.Date,
            CurrentQueueNumber = currentQueueNumber,
            TotalAppointments = appointments.Count,
     BookedCount = appointments.Count(a => a.Status == AppointmentStatus.Booked),
 InProgressCount = appointments.Count(a => a.Status == AppointmentStatus.InProgress),
  CompletedCount = appointments.Count(a => a.Status == AppointmentStatus.Completed),
   CanceledCount = appointments.Count(a => a.Status == AppointmentStatus.Canceled),
         Appointments = appointmentDtos
    };
    }

    public async Task<WeeklyQueueDto> GetClinicWeeklyQueueAsync(int clinicId, DateTime startDate)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
        if (clinic == null)
         throw new KeyNotFoundException($"Clinic with ID {clinicId} not found");

     var endDate = startDate.AddDays(6); // 7 days total
        var appointments = await _unitOfWork.Appointments.GetClinicAppointmentsByDateRangeAsync(clinicId, startDate, endDate);
        
 var dailySummaries = new List<DailyQueueSummaryDto>();
        var busyDays = new List<string>();
        var totalAppointments = 0;

  for (int i = 0; i < 7; i++)
        {
     var currentDate = startDate.AddDays(i);
         var dayOfWeek = (DayOfWeekEnum)((int)currentDate.DayOfWeek);
       var workingDay = await _unitOfWork.WorkingDays.GetWorkingDayAsync(clinicId, dayOfWeek);
       var exception = await _unitOfWork.Exceptions.GetExceptionByDateAsync(clinicId, currentDate);
         
            var dayAppointments = appointments.Where(a => a.AppointmentDate.Date == currentDate.Date).ToList();
            var maxCapacity = await _scheduleService.GetDailyCapacityAsync(clinicId, currentDate);
            var availableSlots = maxCapacity - dayAppointments.Count(a => a.Status != AppointmentStatus.Canceled);

      var summary = new DailyQueueSummaryDto
   {
                Date = currentDate.Date,
   DayName = currentDate.ToString("dddd"),
      TotalAppointments = dayAppointments.Count,
     BookedCount = dayAppointments.Count(a => a.Status == AppointmentStatus.Booked),
         InProgressCount = dayAppointments.Count(a => a.Status == AppointmentStatus.InProgress),
     CompletedCount = dayAppointments.Count(a => a.Status == AppointmentStatus.Completed),
        CanceledCount = dayAppointments.Count(a => a.Status == AppointmentStatus.Canceled),
    MaxCapacity = maxCapacity,
      AvailableSlots = availableSlots,
      IsWorkingDay = workingDay != null && !workingDay.IsClosed,
          HasException = exception != null,
          ExceptionReason = exception?.Reason,
                StartTime = workingDay?.StartTime,
        EndTime = workingDay?.EndTime
 };

   dailySummaries.Add(summary);
     totalAppointments += dayAppointments.Count;

            // Mark as busy if capacity is above 80% or fully booked
       if (maxCapacity > 0 && availableSlots <= maxCapacity * 0.2)
       {
     busyDays.Add(currentDate.ToString("yyyy-MM-dd"));
 }
  }

        return new WeeklyQueueDto
        {
  ClinicId = clinicId,
            ClinicName = clinic.DoctorName,
            DoctorName = clinic.DoctorName,
  StartDate = startDate.Date,
       EndDate = endDate.Date,
        DailySummaries = dailySummaries,
     TotalAppointments = totalAppointments,
            BusyDays = busyDays
        };
    }

  public async Task<PatientAppointmentHistoryDto> GetPatientHistoryAsync(string patientId)
    {
        var appointments = await _unitOfWork.Appointments.GetPatientAppointmentsAsync(patientId);
        
   var patient = await _userManager.FindByIdAsync(patientId);

        return new PatientAppointmentHistoryDto
 {
        TotalAppointments = appointments.Count,
   CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
          CanceledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Canceled),
            Appointments = appointments.Select(a => MapToAppointmentDto(a, patient)).ToList()
        };
    }

    public async Task<List<AppointmentDto>> GetPatientUpcomingAppointmentsAsync(string patientId)
    {
        var appointments = await _unitOfWork.Appointments.GetPatientUpcomingAppointmentsAsync(patientId);
        var patient = await _userManager.FindByIdAsync(patientId);
        return appointments.Select(a => MapToAppointmentDto(a, patient)).ToList();
    }

    public async Task<List<AppointmentDto>> GetPatientPastAppointmentsAsync(string patientId)
    {
        var appointments = await _unitOfWork.Appointments.GetPatientPastAppointmentsAsync(patientId);
  var patient = await _userManager.FindByIdAsync(patientId);
        return appointments.Select(a => MapToAppointmentDto(a, patient)).ToList();
    }

    public async Task<int> GetEstimatedWaitTimeAsync(int appointmentId)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
        if (appointment == null) return 0;

        var clinic = await _unitOfWork.Clinics.GetByIdAsync(appointment.ClinicId);
     if (clinic == null) return 0;

      var currentQueueNumber = await _unitOfWork.Appointments.GetCurrentQueueNumberAsync(
     appointment.ClinicId, appointment.AppointmentDate);

 var peopleAhead = appointment.QueueNumber - currentQueueNumber;
        if (peopleAhead <= 0) return 0;

        return peopleAhead * clinic.SlotDurationMinutes;
    }

  // Mapping helpers
    private AppointmentDto MapToAppointmentDto(Appointment appointment, AppUser? patient)
  {
    return new AppointmentDto
      {
            Id = appointment.Id,
            ClinicId = appointment.ClinicId,
     ClinicName = appointment.Clinic?.DoctorName ?? "Unknown",
     DoctorName = appointment.Clinic?.DoctorName ?? "Unknown",
     Specialty = appointment.Clinic?.Specialty ?? "Unknown",
 PatientId = appointment.PatientId,
          PatientName = patient?.DisplayName,
         PatientPhone = patient?.PhoneNumber,
    AppointmentDate = appointment.AppointmentDate,
 AppointmentTime = appointment.AppointmentTime,
            QueueNumber = appointment.QueueNumber,
  Status = appointment.Status,
      StatusName = appointment.Status.ToString(),
        EstimatedWaitMinutes = 0, // Calculated separately if needed
       CreatedAt = appointment.CreatedAt
     };
    }

    private ClinicAddressDto MapToAddressDto(ClinicAddress address)
    {
 return new ClinicAddressDto
      {
    Id = address.Id,
            Country = address.Country,
    City = address.City,
         Area = address.Area,
            Street = address.Street,
            Building = address.Building,
            Notes = address.Notes
        };
    }

    private ClinicPhoneDto MapToPhoneDto(ClinicPhone phone)
    {
      return new ClinicPhoneDto
        {
            Id = phone.Id,
         PhoneNumber = phone.PhoneNumber
};
    }
}
