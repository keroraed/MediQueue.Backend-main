using MediQueue.Core.Entities.Identity;
using MediQueue.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.APIs.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ClinicsModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public ClinicsModel(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public List<ClinicViewModel> Clinics { get; set; } = new();
    
    [TempData]
    public string? SuccessMessage { get; set; }
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var clinics = await _unitOfWork.Clinics.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();
            var allAddresses = await _unitOfWork.Repository<Core.Entities.ClinicAddress>().GetAllAsync();
            var allRatings = await _unitOfWork.Repository<Core.Entities.ClinicRating>().GetAllAsync();

            foreach (var clinic in clinics)
            {
                var user = users.FirstOrDefault(u => u.Id == clinic.AppUserId);
                var address = allAddresses.FirstOrDefault(a => a.ClinicId == clinic.Id);
                var clinicRatings = allRatings.Where(r => r.ClinicId == clinic.Id).ToList();
                var avgRating = clinicRatings.Any() ? clinicRatings.Average(r => r.Rating) : (double?)null;

                Clinics.Add(new ClinicViewModel
                {
                    Id = clinic.Id,
                    DoctorName = clinic.DoctorName,
                    Specialty = clinic.Specialty,
                    Description = clinic.Description,
                    Email = user?.Email ?? "N/A",
                    PhoneNumber = user?.PhoneNumber,
                    City = address?.City,
                    AverageRating = avgRating,
                    TotalRatings = clinicRatings.Count,
                    TotalAppointments = clinic.Appointments?.Count ?? 0,
                    CreatedDate = user?.DateCreated ?? DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading clinics: {ex.Message}";
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int clinicId)
    {
        try
        {
            var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
            if (clinic == null)
            {
                ErrorMessage = "Clinic not found";
                return RedirectToPage();
            }

            _unitOfWork.Clinics.Delete(clinic);
            await _unitOfWork.Complete();

            SuccessMessage = "Clinic deleted successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting clinic: {ex.Message}";
        }

        return RedirectToPage();
    }
}

public class ClinicViewModel
{
    public int Id { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public double? AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public int TotalAppointments { get; set; }
    public DateTime CreatedDate { get; set; }
}
