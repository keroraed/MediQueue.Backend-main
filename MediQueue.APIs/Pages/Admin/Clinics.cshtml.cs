using MediQueue.Core.Entities.Identity;
using MediQueue.Core.Repositories;
using MediQueue.Repository.Identity;
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
    private readonly AppIdentityDbContext _identityContext;
    private readonly IWebHostEnvironment _env;

    public ClinicsModel(
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        AppIdentityDbContext identityContext,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _identityContext = identityContext;
        _env = env;
    }

    public List<ClinicViewModel> Clinics { get; set; } = new();
    
    [TempData]
    public string? SuccessMessage { get; set; }
    
    [TempData]
    public string? ErrorMessage { get; set; }

    // ── Bind properties for the Add Clinic form ──
    [BindProperty] public string ClinicEmail { get; set; } = null!;
    [BindProperty] public string ClinicDisplayName { get; set; } = null!;
    [BindProperty] public string ClinicPhoneNumber { get; set; } = null!;
    [BindProperty] public string ClinicPassword { get; set; } = null!;
    [BindProperty] public string DoctorName { get; set; } = null!;
    [BindProperty] public string Specialty { get; set; } = null!;
    [BindProperty] public string Description { get; set; } = null!;
    [BindProperty] public int SlotDurationMinutes { get; set; } = 30;
    [BindProperty] public string Country { get; set; } = null!;
    [BindProperty] public string City { get; set; } = null!;
    [BindProperty] public string Area { get; set; } = null!;
    [BindProperty] public string Street { get; set; } = null!;
    [BindProperty] public string Building { get; set; } = null!;
    [BindProperty] public string? AddressNotes { get; set; }
    [BindProperty] public IFormFile? ProfilePicture { get; set; }
    [BindProperty] public decimal? ConsultationFee { get; set; }
    [BindProperty] public List<string>? PaymentMethods { get; set; }

    public async Task OnGetAsync()
    {
        await LoadClinicsAsync();
    }

    /// <summary>
    /// Add a new clinic from the admin dashboard
    /// </summary>
    public async Task<IActionResult> OnPostAddClinicAsync()
    {
        using var identityTransaction = await _identityContext.Database.BeginTransactionAsync();
        try
        {
            // 1. Create Identity user (pre-verified)
            var user = new AppUser
            {
                DisplayName = ClinicDisplayName,
                Email = ClinicEmail,
                UserName = ClinicEmail,
                PhoneNumber = ClinicPhoneNumber,
                DateCreated = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, ClinicPassword);
            if (!result.Succeeded)
            {
                ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                await LoadClinicsAsync();
                return Page();
            }

            await _userManager.AddToRoleAsync(user, "Clinic");
            await identityTransaction.CommitAsync();

            // 2. Handle profile picture upload
            string? profilePictureUrl = null;
            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"),
                    "uploads", "clinic-pictures");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(ProfilePicture.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(stream);
                }
                profilePictureUrl = $"/uploads/clinic-pictures/{fileName}";
            }

            // 3. Create business user
            try
            {
                var businessUser = new Core.Entities.User
                {
                    Email = ClinicEmail,
                    PasswordHash = user.PasswordHash!,
                    Role = "Clinic",
                    IsVerified = true,
                    IsActive = true
                };
                _unitOfWork.Users.Add(businessUser);
                await _unitOfWork.Complete();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
            {
                // Already exists – ignore
            }

            // 4. Create Clinic Profile
            var clinicProfile = new Core.Entities.ClinicProfile
            {
                AppUserId = user.Id,
                DoctorName = DoctorName,
                Specialty = Specialty,
                Description = Description,
                SlotDurationMinutes = SlotDurationMinutes,
                ProfilePictureUrl = profilePictureUrl,
                ConsultationFee = ConsultationFee,
                PaymentMethods = PaymentMethods != null && PaymentMethods.Any()
                    ? string.Join(",", PaymentMethods)
                    : null
            };
            _unitOfWork.Clinics.Add(clinicProfile);
            await _unitOfWork.Complete();

            // 5. Create Clinic Address
            var clinicAddress = new Core.Entities.ClinicAddress
            {
                ClinicId = clinicProfile.Id,
                Country = Country,
                City = City,
                Area = Area,
                Street = Street,
                Building = Building,
                Notes = AddressNotes
            };
            _unitOfWork.Repository<Core.Entities.ClinicAddress>().Add(clinicAddress);
            await _unitOfWork.Complete();

            SuccessMessage = "Clinic created successfully";
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
        {
            await identityTransaction.RollbackAsync();
            ErrorMessage = "Email address or phone number is already in use";
        }
        catch (Exception ex)
        {
            await identityTransaction.RollbackAsync();
            ErrorMessage = $"Error creating clinic: {ex.Message}";
        }

        return RedirectToPage();
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

    private async Task LoadClinicsAsync()
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
                    ProfilePictureUrl = clinic.ProfilePictureUrl,
                    ConsultationFee = clinic.ConsultationFee,
                    PaymentMethods = string.IsNullOrEmpty(clinic.PaymentMethods)
                        ? new List<string>()
                        : clinic.PaymentMethods.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
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
    public string? ProfilePictureUrl { get; set; }
    public decimal? ConsultationFee { get; set; }
    public List<string> PaymentMethods { get; set; } = new();
    public double? AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public int TotalAppointments { get; set; }
    public DateTime CreatedDate { get; set; }
}
