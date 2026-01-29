using MediQueue.Core.DTOs;
using MediQueue.Core.Entities;
using MediQueue.Core.Repositories;
using MediQueue.Core.Services;

namespace MediQueue.Service;

public class ClinicService : IClinicService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClinicService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ClinicProfileDto?> GetClinicProfileAsync(int clinicId)
    {
        var clinic = await _unitOfWork.Clinics.GetClinicWithDetailsAsync(clinicId);
        if (clinic == null) return null;

        return MapToClinicProfileDto(clinic);
    }

    public async Task<ClinicProfileDto?> GetClinicByUserIdAsync(string appUserId)
    {
        var clinic = await _unitOfWork.Clinics.GetClinicByUserIdAsync(appUserId);
        if (clinic == null) return null;

  return MapToClinicProfileDto(clinic);
    }

    public async Task<ClinicProfileDto> CreateClinicProfileAsync(string appUserId, CreateClinicProfileDto dto)
    {
        // Check if clinic already exists for this user
        if (await _unitOfWork.Clinics.ClinicExistsForUserAsync(appUserId))
       throw new InvalidOperationException("Clinic profile already exists for this user");

        var clinic = new ClinicProfile
        {
 AppUserId = appUserId,
       DoctorName = dto.DoctorName,
  Specialty = dto.Specialty,
          Description = dto.Description,
     SlotDurationMinutes = dto.SlotDurationMinutes
        };

_unitOfWork.Clinics.Add(clinic);
        await _unitOfWork.Complete();

        return MapToClinicProfileDto(clinic);
    }

    public async Task<ClinicProfileDto> UpdateClinicProfileAsync(int clinicId, UpdateClinicProfileDto dto)
    {
 var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
      if (clinic == null)
            throw new KeyNotFoundException($"Clinic with ID {clinicId} not found");

        clinic.DoctorName = dto.DoctorName;
        clinic.Specialty = dto.Specialty;
      clinic.Description = dto.Description;
        clinic.SlotDurationMinutes = dto.SlotDurationMinutes;

        _unitOfWork.Clinics.Update(clinic);
        await _unitOfWork.Complete();

        return MapToClinicProfileDto(clinic);
    }

    public async Task<ClinicAddressDto?> GetClinicAddressAsync(int clinicId)
    {
        var clinic = await _unitOfWork.Clinics.GetClinicWithDetailsAsync(clinicId);
        if (clinic?.Address == null) return null;

        return MapToAddressDto(clinic.Address);
    }

    public async Task<ClinicAddressDto> CreateOrUpdateAddressAsync(int clinicId, CreateClinicAddressDto dto)
    {
        var clinic = await _unitOfWork.Clinics.GetClinicWithDetailsAsync(clinicId);
        if (clinic == null)
         throw new KeyNotFoundException($"Clinic with ID {clinicId} not found");

    if (clinic.Address == null)
        {
        // Create new address
 clinic.Address = new ClinicAddress
    {
   ClinicId = clinicId,
       Country = dto.Country,
     City = dto.City,
    Area = dto.Area,
        Street = dto.Street,
        Building = dto.Building,
  Notes = dto.Notes
            };
        }
        else
    {
            // Update existing address
            clinic.Address.Country = dto.Country;
      clinic.Address.City = dto.City;
         clinic.Address.Area = dto.Area;
         clinic.Address.Street = dto.Street;
  clinic.Address.Building = dto.Building;
            clinic.Address.Notes = dto.Notes;
    }

        await _unitOfWork.Complete();
        return MapToAddressDto(clinic.Address);
}

    public async Task<List<ClinicPhoneDto>> GetClinicPhonesAsync(int clinicId)
{
        var clinic = await _unitOfWork.Clinics.GetClinicWithDetailsAsync(clinicId);
        if (clinic == null) return new List<ClinicPhoneDto>();

      return clinic.Phones.Select(MapToPhoneDto).ToList();
    }

    public async Task<ClinicPhoneDto> AddPhoneAsync(int clinicId, CreateClinicPhoneDto dto)
    {
        var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
   if (clinic == null)
            throw new KeyNotFoundException($"Clinic with ID {clinicId} not found");

      var phone = new ClinicPhone
   {
ClinicId = clinicId,
         PhoneNumber = dto.PhoneNumber
   };

        _unitOfWork.Repository<ClinicPhone>().Add(phone);
      await _unitOfWork.Complete();

        return MapToPhoneDto(phone);
    }

    public async Task<ClinicPhoneDto> UpdatePhoneAsync(int phoneId, UpdateClinicPhoneDto dto)
    {
var phone = await _unitOfWork.Repository<ClinicPhone>().GetByIdAsync(phoneId);
      if (phone == null)
            throw new KeyNotFoundException($"Phone with ID {phoneId} not found");

      phone.PhoneNumber = dto.PhoneNumber;

        _unitOfWork.Repository<ClinicPhone>().Update(phone);
        await _unitOfWork.Complete();

        return MapToPhoneDto(phone);
    }

    public async Task DeletePhoneAsync(int phoneId)
 {
        var phone = await _unitOfWork.Repository<ClinicPhone>().GetByIdAsync(phoneId);
        if (phone == null)
            throw new KeyNotFoundException($"Phone with ID {phoneId} not found");

    _unitOfWork.Repository<ClinicPhone>().Delete(phone);
        await _unitOfWork.Complete();
    }

    public async Task<List<ClinicProfileDto>> SearchClinicsAsync(string? specialty, string? city, double? minRating)
 {
        var clinics = await _unitOfWork.Clinics.SearchClinicsAsync(specialty, city, minRating);
    return clinics.Select(MapToClinicProfileDto).ToList();
    }

    public async Task<List<ClinicProfileDto>> GetClinicsBySpecialtyAsync(string specialty)
    {
        var clinics = await _unitOfWork.Clinics.GetClinicsBySpecialtyAsync(specialty);
        return clinics.Select(MapToClinicProfileDto).ToList();
    }

    public async Task<List<ClinicProfileDto>> GetClinicsByCityAsync(string city)
 {
        var clinics = await _unitOfWork.Clinics.GetClinicsByCityAsync(city);
        return clinics.Select(MapToClinicProfileDto).ToList();
    }

    // Mapping helpers
    private ClinicProfileDto MapToClinicProfileDto(ClinicProfile clinic)
    {
        var avgRating = clinic.Ratings.Any() ? clinic.Ratings.Average(r => r.Rating) : 0;

        return new ClinicProfileDto
        {
            Id = clinic.Id,
            UserId = clinic.AppUserId,
            DoctorName = clinic.DoctorName,
            Specialty = clinic.Specialty,
            Description = clinic.Description,
     SlotDurationMinutes = clinic.SlotDurationMinutes,
        Address = clinic.Address != null ? MapToAddressDto(clinic.Address) : null,
        Phones = clinic.Phones.Select(MapToPhoneDto).ToList(),
        WorkingDays = clinic.WorkingDays.Select(MapToWorkingDayDto).OrderBy(w => w.DayOfWeek).ToList(),
     AverageRating = Math.Round(avgRating, 2),
        TotalRatings = clinic.Ratings.Count,
    CreatedAt = clinic.CreatedAt
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
}
