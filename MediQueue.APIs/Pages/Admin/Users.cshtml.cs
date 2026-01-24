using MediQueue.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.APIs.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UsersModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;

    public UsersModel(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public List<UserViewModel> Users { get; set; } = new();
    public string SelectedRole { get; set; } = "all";
    public string SelectedStatus { get; set; } = "all";
    
    [TempData]
    public string? SuccessMessage { get; set; }
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(string? role, string? status)
    {
        SelectedRole = role ?? "all";
        SelectedStatus = status ?? "all";

        try
        {
            var users = await _userManager.Users.ToListAsync();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var userRole = userRoles.FirstOrDefault() ?? "Patient";

                // Apply role filter
                if (!string.IsNullOrEmpty(role) && role != "all" && userRole != role)
                    continue;

                // Apply status filter
                if (!string.IsNullOrEmpty(status) && status != "all")
                {
                    if (status == "locked" && !await _userManager.IsLockedOutAsync(user))
                        continue;
                    if (status == "active" && await _userManager.IsLockedOutAsync(user))
                        continue;
                }

                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    DisplayName = user.DisplayName,
                    PhoneNumber = user.PhoneNumber,
                    Role = userRole,
                    IsEmailConfirmed = user.EmailConfirmed,
                    IsLocked = await _userManager.IsLockedOutAsync(user),
                    DateCreated = user.DateCreated,
                    LastLoginDate = user.LastLoginDate
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading users: {ex.Message}";
        }
    }

    public async Task<IActionResult> OnPostLockAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ErrorMessage = "User not found";
                return RedirectToPage();
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                ErrorMessage = "Cannot lock admin accounts";
                return RedirectToPage();
            }

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            SuccessMessage = "User account locked successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error locking account: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnlockAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ErrorMessage = "User not found";
                return RedirectToPage();
            }

            await _userManager.SetLockoutEndDateAsync(user, null);
            SuccessMessage = "User account unlocked successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error unlocking account: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ErrorMessage = "User not found";
                return RedirectToPage();
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                ErrorMessage = "Cannot delete admin accounts";
                return RedirectToPage();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ErrorMessage = "Failed to delete user";
                return RedirectToPage();
            }

            SuccessMessage = "User deleted successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting user: {ex.Message}";
        }

        return RedirectToPage();
    }
}

public class UserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }
    public bool IsLocked { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? LastLoginDate { get; set; }
}
