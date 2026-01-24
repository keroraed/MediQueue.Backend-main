using MediQueue.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace MediQueue.APIs.Pages.Admin;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        // If already logged in, redirect to dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect("/Admin/Index");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
        {
            ErrorMessage = "Invalid email or password";
            return Page();
        }

        // Check if user is admin
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Admin"))
        {
            ErrorMessage = "You do not have permission to access the admin area";
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Password, RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Redirect("/Admin/Index");
        }

        if (result.IsLockedOut)
        {
            ErrorMessage = "Your account has been locked";
            return Page();
        }

        ErrorMessage = "Invalid email or password";
        return Page();
    }
}
