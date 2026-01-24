using MediQueue.Core.DTOs;

namespace MediQueue.Core.Services;

public interface IClinicService
{
    // Profile Management
    Task<ClinicProfileDto?> GetClinicProfileAsync(int clinicId);
    Task<ClinicProfileDto?> GetClinicByUserIdAsync(string appUserId);
    Task<ClinicProfileDto> CreateClinicProfileAsync(string appUserId, CreateClinicProfileDto dto);
    Task<ClinicProfileDto> UpdateClinicProfileAsync(int clinicId, UpdateClinicProfileDto dto);
 
    // Address Management
    Task<ClinicAddressDto?> GetClinicAddressAsync(int clinicId);
    Task<ClinicAddressDto> CreateOrUpdateAddressAsync(int clinicId, CreateClinicAddressDto dto);
    
    // Phone Management
    Task<List<ClinicPhoneDto>> GetClinicPhonesAsync(int clinicId);
  Task<ClinicPhoneDto> AddPhoneAsync(int clinicId, CreateClinicPhoneDto dto);
    Task<ClinicPhoneDto> UpdatePhoneAsync(int phoneId, UpdateClinicPhoneDto dto);
    Task DeletePhoneAsync(int phoneId);
    
    // Search & Discovery
    Task<List<ClinicProfileDto>> SearchClinicsAsync(string? specialty, string? city, double? minRating);
    Task<List<ClinicProfileDto>> GetClinicsBySpecialtyAsync(string specialty);
    Task<List<ClinicProfileDto>> GetClinicsByCityAsync(string city);
}
