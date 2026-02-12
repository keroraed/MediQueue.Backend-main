using MediQueue.Core.DTOs;
using MediQueue.Core.Entities;
using MediQueue.Core.Enums;
using MediQueue.Core.Repositories;
using MediQueue.Core.Services;

namespace MediQueue.Service;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkingScheduleService _scheduleService;

    public AppointmentService(IUnitOfWork unitOfWork, IWorkingScheduleService scheduleService)
    {
  _unitOfWork = unitOfWork;
   _scheduleService = scheduleService;
    }

    public async Task<AppointmentDto> BookAppointmentAsync(string patientId, BookAppointmentDto dto)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(dto.ClinicId);
        if (clinic == null)
            throw new KeyNotFoundException($"Clinic with ID {dto.ClinicId} not found");

        // Validate date is in the future
        if (dto.AppointmentDate.Date < DateTime.UtcNow.Date)
            throw new InvalidOperationException("Cannot book appointments in the past");

        // Check if clinic is available on this date
        if (!await _scheduleService.IsClinicAvailableAsync(dto.ClinicId, dto.AppointmentDate))
            throw new InvalidOperationException("Clinic is not available on this date");

        // Check capacity
        var capacity = await _scheduleService.GetDailyCapacityAsync(dto.ClinicId, dto.AppointmentDate);
        var currentCount = await _unitOfWork.Appointments.CountAppointmentsByDateAsync(dto.ClinicId, dto.AppointmentDate);

        if (currentCount >= capacity)
            throw new InvalidOperationException("Clinic capacity is full for this date");

        // Get next queue number with row-level locking to prevent race conditions
        var queueNumber = await _unitOfWork.Appointments.GetNextQueueNumberWithLockAsync(dto.ClinicId, dto.AppointmentDate);

        var appointment = new Appointment
        {
            ClinicId = dto.ClinicId,
            PatientId = patientId,
   AppointmentDate = dto.AppointmentDate, // ? FIXED: Store full DateTime including time
    QueueNumber = queueNumber,
          Status = AppointmentStatus.Booked
     };

        _unitOfWork.Appointments.Add(appointment);
        await _unitOfWork.Complete();

      var created = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(appointment.Id);
    return MapToAppointmentDto(created!);
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

    public async Task<AppointmentDto> UpdateAppointmentStatusAsync(int appointmentId, UpdateAppointmentStatusDto dto)
    {
   var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
        if (appointment == null)
    throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");

        appointment.Status = dto.Status;
   _unitOfWork.Appointments.Update(appointment);
  await _unitOfWork.Complete();

    var updated = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(appointmentId);
 return MapToAppointmentDto(updated!);
    }

    public async Task<AppointmentDto> CallNextPatientAsync(int clinicId, DateTime date)
    {
   var appointments = await _unitOfWork.Appointments.GetClinicAppointmentsByDateAsync(clinicId, date);
        
 var nextAppointment = appointments
            .Where(a => a.Status == AppointmentStatus.Booked)
       .OrderBy(a => a.QueueNumber)
    .FirstOrDefault();

      if (nextAppointment == null)
       throw new InvalidOperationException("No booked appointments found");

   nextAppointment.Status = AppointmentStatus.InProgress;
        _unitOfWork.Appointments.Update(nextAppointment);
        await _unitOfWork.Complete();

   var updated = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(nextAppointment.Id);
        return MapToAppointmentDto(updated!);
    }

    public async Task<AppointmentDto> MarkInProgressAsync(int appointmentId)
    {
   return await UpdateAppointmentStatusAsync(appointmentId, new UpdateAppointmentStatusDto 
        { 
     Status = AppointmentStatus.InProgress 
        });
    }

    public async Task<AppointmentDto> MarkCompletedAsync(int appointmentId)
    {
        return await UpdateAppointmentStatusAsync(appointmentId, new UpdateAppointmentStatusDto 
        { 
       Status = AppointmentStatus.Completed 
        });
    }

    public async Task<AppointmentDto> MarkDelayedAsync(int appointmentId)
    {
   return await UpdateAppointmentStatusAsync(appointmentId, new UpdateAppointmentStatusDto 
        { 
     Status = AppointmentStatus.Delayed 
 });
    }

    public async Task<AppointmentDetailsDto?> GetAppointmentDetailsAsync(int appointmentId)
    {
      var appointment = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(appointmentId);
        if (appointment == null) return null;

  var currentQueueNumber = await _unitOfWork.Appointments.GetCurrentQueueNumberAsync(
  appointment.ClinicId, appointment.AppointmentDate);

        var peopleAhead = appointment.QueueNumber - currentQueueNumber;
        if (peopleAhead < 0) peopleAhead = 0;

        var estimatedWait = await GetEstimatedWaitTimeAsync(appointmentId);

   return new AppointmentDetailsDto
 {
       Id = appointment.Id,
  ClinicId = appointment.ClinicId,
     ClinicName = appointment.Clinic.DoctorName,
        DoctorName = appointment.Clinic.DoctorName,
       Specialty = appointment.Clinic.Specialty,
    Address = appointment.Clinic.Address != null ? MapToAddressDto(appointment.Clinic.Address) : null,
Phones = appointment.Clinic.Phones.Select(MapToPhoneDto).ToList(),
    PatientId = appointment.PatientId,
    AppointmentDate = appointment.AppointmentDate,
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
         Appointments = appointments.Select(MapToAppointmentDto).ToList()
      };
    }

    public async Task<PatientAppointmentHistoryDto> GetPatientHistoryAsync(string patientId)
{
        var appointments = await _unitOfWork.Appointments.GetPatientAppointmentsAsync(patientId);

        return new PatientAppointmentHistoryDto
   {
            TotalAppointments = appointments.Count,
   CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
       CanceledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Canceled),
      Appointments = appointments.Select(MapToAppointmentDto).ToList()
   };
    }

    public async Task<List<AppointmentDto>> GetPatientUpcomingAppointmentsAsync(string patientId)
 {
        var appointments = await _unitOfWork.Appointments.GetPatientUpcomingAppointmentsAsync(patientId);
  return appointments.Select(MapToAppointmentDto).ToList();
    }

  public async Task<List<AppointmentDto>> GetPatientPastAppointmentsAsync(string patientId)
    {
   var appointments = await _unitOfWork.Appointments.GetPatientPastAppointmentsAsync(patientId);
        return appointments.Select(MapToAppointmentDto).ToList();
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
    private AppointmentDto MapToAppointmentDto(Appointment appointment)
    {
   return new AppointmentDto
     {
     Id = appointment.Id,
          ClinicId = appointment.ClinicId,
            ClinicName = appointment.Clinic?.DoctorName ?? "Unknown",
      DoctorName = appointment.Clinic?.DoctorName ?? "Unknown",
            Specialty = appointment.Clinic?.Specialty ?? "Unknown",
PatientId = appointment.PatientId,
       AppointmentDate = appointment.AppointmentDate,
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
