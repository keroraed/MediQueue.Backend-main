using MediQueue.Core.Entities.Identity;
using MediQueue.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.APIs.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AppointmentsModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentsModel(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public List<AppointmentViewModel> Appointments { get; set; } = new();
    public string SelectedStatus { get; set; } = "all";
    public string SelectedDateFilter { get; set; } = "all";

    public async Task OnGetAsync(string? status, string? dateFilter)
    {
        SelectedStatus = status ?? "all";
        SelectedDateFilter = dateFilter ?? "all";

        try
        {
            var appointments = await _unitOfWork.Repository<Core.Entities.Appointment>().GetAllAsync();
            var clinics = await _unitOfWork.Clinics.GetAllAsync();
            var appUsers = await _userManager.Users.ToListAsync();

            var query = appointments.AsQueryable();

            // Apply status filter
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                if (Enum.TryParse<Core.Enums.AppointmentStatus>(status, out var statusEnum))
                {
                    query = query.Where(a => a.Status == statusEnum);
                }
            }

            // Apply date filter
            if (!string.IsNullOrEmpty(dateFilter))
            {
                var today = DateTime.UtcNow.Date;
                query = dateFilter switch
                {
                    "today" => query.Where(a => a.AppointmentDate.Date == today),
                    "week" => query.Where(a => a.AppointmentDate.Date >= today && a.AppointmentDate.Date <= today.AddDays(7)),
                    "month" => query.Where(a => a.AppointmentDate.Date >= today && a.AppointmentDate.Date <= today.AddDays(30)),
                    _ => query
                };
            }

            Appointments = query
                .OrderByDescending(a => a.CreatedAt)
                .ToList()
                .Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    PatientName = appUsers.FirstOrDefault(u => u.Id == a.PatientId)?.DisplayName ?? "Unknown Patient",
                    ClinicName = clinics.FirstOrDefault(c => c.Id == a.ClinicId)?.DoctorName ?? "Unknown Clinic",
                    AppointmentDate = a.AppointmentDate,
                    QueueNumber = a.QueueNumber,
                    Status = a.Status.ToString(),
                    CreatedAt = a.CreatedAt
                })
                .ToList();
        }
        catch (Exception)
        {
            Appointments = new();
        }
    }
}

public class AppointmentViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public int QueueNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
