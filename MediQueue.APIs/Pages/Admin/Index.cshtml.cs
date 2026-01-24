using MediQueue.Core.Entities.Identity;
using MediQueue.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.APIs.Pages.Admin;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public int TotalClinics { get; set; }
    public int TotalPatients { get; set; }
    public int TotalAppointments { get; set; }
    public int TodayAppointments { get; set; }
    public List<RecentAppointmentViewModel> RecentAppointments { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            var allClinics = await _unitOfWork.Clinics.GetAllAsync();
            TotalClinics = allClinics.Count();

            TotalPatients = await _userManager.Users.CountAsync(u => u.EmailConfirmed);

            var allAppointments = await _unitOfWork.Repository<Core.Entities.Appointment>().GetAllAsync();
            TotalAppointments = allAppointments.Count();
            TodayAppointments = allAppointments.Count(a => a.AppointmentDate.Date == DateTime.UtcNow.Date);

            var clinics = await _unitOfWork.Clinics.GetAllAsync();
            var appUsers = await _userManager.Users.ToListAsync();

            RecentAppointments = allAppointments
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToList()
                .Select(a => new RecentAppointmentViewModel
                {
                    Id = a.Id,
                    PatientName = appUsers.FirstOrDefault(u => u.Id == a.PatientId)?.DisplayName ?? "Unknown Patient",
                    ClinicName = clinics.FirstOrDefault(c => c.Id == a.ClinicId)?.DoctorName ?? "Unknown Clinic",
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status.ToString()
                })
                .ToList();
        }
        catch (Exception)
        {
            // Handle errors gracefully
            TotalClinics = 0;
            TotalPatients = 0;
            TotalAppointments = 0;
            TodayAppointments = 0;
            RecentAppointments = new();
        }
    }
}

public class RecentAppointmentViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
