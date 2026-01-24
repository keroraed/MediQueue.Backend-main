using AutoMapper;
using MediQueue.APIs.Errors;
using MediQueue.Core.DTOs;
using MediQueue.Core.Entities.Identity;
using MediQueue.Core.Enums;
using MediQueue.Core.Repositories;
using MediQueue.Core.Services;
using MediQueue.Core.Settings;
using MediQueue.Repository.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace MediQueue.APIs.Controllers;

public class AccountController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly IOtpRepository _otpRepository;
    private readonly IMapper _mapper;
    private readonly AppIdentityDbContext _identityContext;
    private readonly IClinicService _clinicService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OtpSettings _otpSettings;
    private readonly IUserAddressService _userAddressService;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
        IOtpService otpService,
        IEmailService emailService,
        IOtpRepository otpRepository,
        IMapper mapper,
        AppIdentityDbContext identityContext,
        IClinicService clinicService,
        IUnitOfWork unitOfWork,
        IOptions<OtpSettings> otpSettings,
        IUserAddressService userAddressService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _otpService = otpService;
        _emailService = emailService;
        _otpRepository = otpRepository;
        _mapper = mapper;
        _identityContext = identityContext;
        _clinicService = clinicService;
        _unitOfWork = unitOfWork;
        _otpSettings = otpSettings.Value;
        _userAddressService = userAddressService;
    }

    /// <summary>
    /// Register as Patient with patient-specific fields
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register/patient")]
    public async Task<ActionResult> RegisterPatient(RegisterPatientDTO registerDto)
    {
        // Use transaction to ensure atomicity across both databases
        using var identityTransaction = await _identityContext.Database.BeginTransactionAsync();
        try
        {
            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                DateCreated = DateTime.UtcNow,
                EmailConfirmed = false,
                // Patient-specific fields
                DateOfBirth = registerDto.DateOfBirth,
                Gender = registerDto.Gender,
                BloodType = registerDto.BloodType,
                EmergencyContact = registerDto.EmergencyContact,
                EmergencyContactPhone = registerDto.EmergencyContactPhone
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                // Check for duplicate errors
                var duplicateError = result.Errors.FirstOrDefault(e => 
                    e.Code == "DuplicateUserName" || e.Code == "DuplicateEmail");
                
                if (duplicateError != null)
                    return BadRequest(new ApiResponse(400, "Email address is already in use"));
                
                return BadRequest(new ApiResponse(400, string.Join(", ", result.Errors.Select(e => e.Description))));
            }

            // Assign Patient role
            await _userManager.AddToRoleAsync(user, "Patient");

            // Generate and send email verification OTP (do this before business DB operations)
            await _otpRepository.InvalidateAllOtpsByEmailAsync(registerDto.Email, OtpPurpose.EmailVerification);

            var otpCode = _otpService.GenerateOtp();
            var hashedOtp = _otpService.HashOtp(otpCode);

            var otp = new Otp
            {
                Email = registerDto.Email,
                Code = hashedOtp,
                ExpirationDate = DateTime.UtcNow.AddMinutes(_otpSettings.ExpirationMinutes),
                IsUsed = false,
                FailedAttempts = 0,
                Purpose = OtpPurpose.EmailVerification
            };

            await _otpRepository.AddOtpAsync(otp);
            await _emailService.SendEmailVerificationOtpAsync(registerDto.Email, otpCode, registerDto.DisplayName);

            // Commit Identity transaction first
            await identityTransaction.CommitAsync();

            // Now create User entity in business DB (outside transaction - acceptable for academic project)
            // If this fails, user can still login but won't have business profile (can be handled manually)
            try
            {
                var businessUser = new Core.Entities.User
                {
                    Email = registerDto.Email,
                    PasswordHash = user.PasswordHash!,
                    Role = "Patient",
                    IsVerified = false,
                    IsActive = true
                };
                _unitOfWork.Users.Add(businessUser);
                await _unitOfWork.Complete();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
            {
                // User already exists in business DB, this is OK (might be from previous failed attempt)
            }

            return Ok(new ApiResponse(200, "Patient registration successful. Please check your email to verify your account."));
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
        {
            await identityTransaction.RollbackAsync();
            return BadRequest(new ApiResponse(400, "Email address or phone number is already in use"));
        }
        catch (Exception)
        {
            await identityTransaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Register as Clinic with clinic profile and address
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register/clinic")]
    public async Task<ActionResult> RegisterClinic(RegisterClinicDTO registerDto)
    {
        // Use transaction to ensure atomicity
        using var identityTransaction = await _identityContext.Database.BeginTransactionAsync();
        
        try
        {
            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                DateCreated = DateTime.UtcNow,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                // Check for duplicate errors
                var duplicateError = result.Errors.FirstOrDefault(e => 
                    e.Code == "DuplicateUserName" || e.Code == "DuplicateEmail");
                
                if (duplicateError != null)
                    return BadRequest(new ApiResponse(400, "Email address is already in use"));
                
                return BadRequest(new ApiResponse(400, string.Join(", ", result.Errors.Select(e => e.Description))));
            }

            // Assign Clinic role
            await _userManager.AddToRoleAsync(user, "Clinic");

            // Generate and send email verification OTP
            await _otpRepository.InvalidateAllOtpsByEmailAsync(registerDto.Email, OtpPurpose.EmailVerification);

            var otpCode = _otpService.GenerateOtp();
            var hashedOtp = _otpService.HashOtp(otpCode);

            var otp = new Otp
            {
                Email = registerDto.Email,
                Code = hashedOtp,
                ExpirationDate = DateTime.UtcNow.AddMinutes(_otpSettings.ExpirationMinutes),
                IsUsed = false,
                FailedAttempts = 0,
                Purpose = OtpPurpose.EmailVerification
            };

            await _otpRepository.AddOtpAsync(otp);
            await _emailService.SendEmailVerificationOtpAsync(registerDto.Email, otpCode, registerDto.DisplayName);

            // Commit identity transaction
            await identityTransaction.CommitAsync();

            // Now create business entities (outside transaction - acceptable for academic project)
            // If these fail, user can still login but won't have clinic profile (can be handled manually)
            try
            {
                // Create User entity in business DB (consistent with Patient registration)
                var businessUser = new Core.Entities.User
                {
                    Email = registerDto.Email,
                    PasswordHash = user.PasswordHash!,
                    Role = "Clinic",
                    IsVerified = false,
                    IsActive = true
                };
                _unitOfWork.Users.Add(businessUser);
                await _unitOfWork.Complete();

                // Create Clinic Profile
                var clinicProfile = new Core.Entities.ClinicProfile
                {
                    AppUserId = user.Id,
                    DoctorName = registerDto.DoctorName,
                    Specialty = registerDto.Specialty,
                    Description = registerDto.Description,
                    SlotDurationMinutes = registerDto.SlotDurationMinutes
                };
                _unitOfWork.Clinics.Add(clinicProfile);
                await _unitOfWork.Complete();

                // Create Clinic Address
                var clinicAddress = new Core.Entities.ClinicAddress
                {
                    ClinicId = clinicProfile.Id,
                    Country = registerDto.Country,
                    City = registerDto.City,
                    Area = registerDto.Area,
                    Street = registerDto.Street,
                    Building = registerDto.Building,
                    Notes = registerDto.AddressNotes
                };
                _unitOfWork.Repository<Core.Entities.ClinicAddress>().Add(clinicAddress);
                await _unitOfWork.Complete();

                // Create additional phones if provided
                if (registerDto.AdditionalPhones != null && registerDto.AdditionalPhones.Any())
                {
                    foreach (var phoneNumber in registerDto.AdditionalPhones)
                    {
                        var clinicPhone = new Core.Entities.ClinicPhone
                        {
                            ClinicId = clinicProfile.Id,
                            PhoneNumber = phoneNumber
                        };
                        _unitOfWork.Repository<Core.Entities.ClinicPhone>().Add(clinicPhone);
                    }
                    await _unitOfWork.Complete();
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
            {
                // User already exists in business DB, this is OK (might be from previous failed attempt)
            }

            return Ok(new ApiResponse(200, "Clinic registration successful. Please check your email to verify your account."));
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
        {
            await identityTransaction.RollbackAsync();
            return BadRequest(new ApiResponse(400, "Email address or phone number is already in use"));
        }
        catch (Exception)
        {
            await identityTransaction.RollbackAsync();
            throw;
        }
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
 public async Task<ActionResult<UserDTO>> VerifyEmail(VerifyEmailDto verifyEmailDto)
    {
     var user = await _userManager.FindByEmailAsync(verifyEmailDto.Email);

        if (user == null)
        {
 return BadRequest(new ApiResponse(400, "User not found"));
        }

        if (user.EmailConfirmed)
        {
 return BadRequest(new ApiResponse(400, "Email is already verified"));
     }

        var otp = await _otpRepository.GetOtpByEmailAsync(verifyEmailDto.Email, OtpPurpose.EmailVerification);

      if (otp == null)
     {
            return BadRequest(new ApiResponse(400, "Invalid or expired OTP"));
        }

        // Check if locked due to too many failed attempts
     if (otp.LockedUntil.HasValue && otp.LockedUntil > DateTime.UtcNow)
        {
return BadRequest(new ApiResponse(400, "Too many failed attempts. Please request a new OTP"));
        }

      // Verify OTP
        if (!_otpService.VerifyOtp(verifyEmailDto.OtpCode, otp.Code))
        {
            otp.FailedAttempts++;

     if (otp.FailedAttempts >= _otpSettings.MaxFailedAttempts)
   {
       otp.LockedUntil = DateTime.UtcNow.AddMinutes(_otpSettings.LockoutDurationMinutes);
}

            await _otpRepository.UpdateOtpAsync(otp);
   return BadRequest(new ApiResponse(400, "Invalid OTP code"));
  }

   // Mark OTP as used
        otp.IsUsed = true;
        await _otpRepository.UpdateOtpAsync(otp);

        // Confirm email
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        // Update IsVerified in business database
        try
        {
            var businessUser = await _unitOfWork.Users.GetByEmailAsync(verifyEmailDto.Email);
            if (businessUser != null)
            {
                businessUser.IsVerified = true;
                _unitOfWork.Users.Update(businessUser);
                await _unitOfWork.Complete();
            }
        }
        catch (Exception)
        {
            // If business user doesn't exist or update fails, still allow login
            // This is acceptable for academic project - user can use system
        }

        // Generate JWT token with single role
        var roles = await _userManager.GetRolesAsync(user);
var role = roles.FirstOrDefault() ?? "Patient"; // Single role only

        return Ok(new UserDTO
        {
            DisplayName = user.DisplayName,
          Email = user.Email!,
  Token = _tokenService.CreateToken(user, roles),
      Role = role
    });
    }

    [AllowAnonymous]
    [HttpPost("resend-verification-otp")]
    public async Task<ActionResult> ResendVerificationOtp(ResendOtpDto resendOtpDto)
    {
        var user = await _userManager.FindByEmailAsync(resendOtpDto.Email);

        if (user == null)
     {
  return Ok(new ApiResponse(200, "If the email exists, a verification OTP has been sent"));
  }

        if (user.EmailConfirmed)
        {
 return BadRequest(new ApiResponse(400, "Email is already verified"));
        }

  // Implement rate limiting (60-second cooldown)
        var existingOtp = await _otpRepository.GetOtpByEmailAsync(resendOtpDto.Email, OtpPurpose.EmailVerification);

        if (existingOtp != null)
    {
        var timeSinceLastOtp = (DateTime.UtcNow - existingOtp.CreatedAt).TotalSeconds;
            if (timeSinceLastOtp < _otpSettings.ResendCooldownSeconds)
   {
  return BadRequest(new ApiResponse(400, $"Please wait {_otpSettings.ResendCooldownSeconds} seconds before requesting a new OTP"));
            }
        }

        // Invalidate previous email verification OTPs
        await _otpRepository.InvalidateAllOtpsByEmailAsync(resendOtpDto.Email, OtpPurpose.EmailVerification);

      // Generate new OTP
        var otpCode = _otpService.GenerateOtp();
        var hashedOtp = _otpService.HashOtp(otpCode);

        var otp = new Otp
        {
    Email = resendOtpDto.Email,
            Code = hashedOtp,
 ExpirationDate = DateTime.UtcNow.AddMinutes(_otpSettings.ExpirationMinutes),
     IsUsed = false,
            FailedAttempts = 0,
      Purpose = OtpPurpose.EmailVerification
      };

        await _otpRepository.AddOtpAsync(otp);

        // Send verification email
        await _emailService.SendEmailVerificationOtpAsync(resendOtpDto.Email, otpCode, user.DisplayName);

        return Ok(new ApiResponse(200, "A new verification OTP has been sent to your email"));
    }

    /// <summary>
    /// Login for all users (Patient, Clinic, Admin)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user == null)
        {
            return Unauthorized(new ApiResponse(401, "Invalid email or password"));
        }

        // Check if email is confirmed (skip for Admin)
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Patient";
        
        if (!user.EmailConfirmed && role != "Admin")
        {
            return Unauthorized(new ApiResponse(401, "Please verify your email address before logging in"));
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (!result.Succeeded)
        {
            return Unauthorized(new ApiResponse(401, "Invalid email or password"));
        }

        // Update last login date
        user.LastLoginDate = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Ok(new UserDTO
        {
            DisplayName = user.DisplayName,
            Email = user.Email!,
            Token = _tokenService.CreateToken(user, roles),
            Role = role
        });
    }

    [Authorize]
    [HttpGet("GetCurrentUser")]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);

        var user = await _userManager.FindByEmailAsync(email!);

    if (user == null)
        {
            return NotFound(new ApiResponse(404, "User not found"));
        }

var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Patient"; // Single role only

        return Ok(new UserDTO
        {
  DisplayName = user.DisplayName,
            Email = user.Email!,
            Token = _tokenService.CreateToken(user, roles),
            Role = role
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new ApiResponse(200, "Logged out successfully"));
    }

    [AllowAnonymous]
    [HttpPost("ForgotPassword")]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

        // Don't reveal whether the user exists or not (prevent email enumeration)
   if (user == null)
   {
       return Ok(new ApiResponse(200, "If the email exists, an OTP has been sent"));
 }

    // Invalidate all previous OTPs for this email
        await _otpRepository.InvalidateAllOtpsByEmailAsync(forgotPasswordDto.Email, OtpPurpose.PasswordReset);

      // Generate OTP
 var otpCode = _otpService.GenerateOtp();
    var hashedOtp = _otpService.HashOtp(otpCode);

        // Store OTP in database
        var otp = new Otp
        {
            Email = forgotPasswordDto.Email,
      Code = hashedOtp,
            ExpirationDate = DateTime.UtcNow.AddMinutes(_otpSettings.ExpirationMinutes),
       IsUsed = false,
   FailedAttempts = 0,
            Purpose = OtpPurpose.PasswordReset
        };

   await _otpRepository.AddOtpAsync(otp);

        // Send OTP email
        await _emailService.SendOtpEmailAsync(forgotPasswordDto.Email, otpCode);

    return Ok(new ApiResponse(200, "If the email exists, an OTP has been sent"));
    }

    [AllowAnonymous]
    [HttpPost("VerifyOtp")]
    public async Task<ActionResult<OtpVerificationResponseDto>> VerifyOtp(VerifyOtpDto verifyOtpDto)
    {
     var otp = await _otpRepository.GetOtpByEmailAsync(verifyOtpDto.Email, OtpPurpose.PasswordReset);

        if (otp == null)
        {
      return BadRequest(new { Success = false, Message = "Invalid or expired OTP" });
        }

    // Check if locked due to too many failed attempts
      if (otp.LockedUntil.HasValue && otp.LockedUntil > DateTime.UtcNow)
        {
       return BadRequest(new { Success = false, Message = "Too many failed attempts. Please try again later" });
    }

        // Verify OTP
        if (!_otpService.VerifyOtp(verifyOtpDto.OtpCode, otp.Code))
      {
 otp.FailedAttempts++;

   if (otp.FailedAttempts >= _otpSettings.MaxFailedAttempts)
            {
         otp.LockedUntil = DateTime.UtcNow.AddMinutes(_otpSettings.LockoutDurationMinutes);
      }

  await _otpRepository.UpdateOtpAsync(otp);
 return BadRequest(new { Success = false, Message = "Invalid OTP code" });
        }

        // Generate reset token
        var resetToken = _otpService.GenerateResetToken();
        otp.ResetToken = resetToken;
        otp.ResetTokenExpiration = DateTime.UtcNow.AddMinutes(5);
        otp.IsUsed = true;

     await _otpRepository.UpdateOtpAsync(otp);

        return Ok(new OtpVerificationResponseDto
  {
       Success = true,
     Message = "OTP verified successfully",
      ResetToken = resetToken
        });
    }

    [AllowAnonymous]
    [HttpPost("ResetPasswordWithToken")]
    public async Task<ActionResult> ResetPasswordWithToken(ResetPasswordWithTokenDto resetPasswordDto)
    {
        var otp = await _otpRepository.GetOtpByResetTokenAsync(resetPasswordDto.ResetToken);

        if (otp == null || otp.Email != resetPasswordDto.Email)
        {
            return BadRequest(new ApiResponse(400, "Invalid or expired reset token"));
        }

        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);

      if (user == null)
  {
            return BadRequest(new ApiResponse(400, "User not found"));
        }

        // Generate password reset token
   var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Reset password
        var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword);

        if (!result.Succeeded)
        {
     return BadRequest(new ApiResponse(400, string.Join(", ", result.Errors.Select(e => e.Description))));
      }

        // Invalidate the reset token
        otp.ResetToken = null;
        otp.ResetTokenExpiration = null;
        await _otpRepository.UpdateOtpAsync(otp);

        return Ok(new ApiResponse(200, "Password has been reset successfully"));
    }

    [AllowAnonymous]
    [HttpPost("ResendOtp")]
    public async Task<ActionResult> ResendOtp(ResendOtpDto resendOtpDto)
    {
  // Implement rate limiting (60-second cooldown)
        var existingOtp = await _otpRepository.GetOtpByEmailAsync(resendOtpDto.Email, OtpPurpose.PasswordReset);

        if (existingOtp != null)
        {
    var timeSinceLastOtp = (DateTime.UtcNow - existingOtp.CreatedAt).TotalSeconds;
   if (timeSinceLastOtp < _otpSettings.ResendCooldownSeconds)
   {
   return BadRequest(new ApiResponse(400, $"Please wait {_otpSettings.ResendCooldownSeconds} seconds before requesting a new OTP"));
        }
     }

   // Invalidate previous OTPs
        await _otpRepository.InvalidateAllOtpsByEmailAsync(resendOtpDto.Email, OtpPurpose.PasswordReset);

  // Generate new OTP
        var otpCode = _otpService.GenerateOtp();
        var hashedOtp = _otpService.HashOtp(otpCode);

      var otp = new Otp
        {
            Email = resendOtpDto.Email,
            Code = hashedOtp,
          ExpirationDate = DateTime.UtcNow.AddMinutes(_otpSettings.ExpirationMinutes),
            IsUsed = false,
         FailedAttempts = 0,
  Purpose = OtpPurpose.PasswordReset
        };

        await _otpRepository.AddOtpAsync(otp);

        // Send OTP email
        await _emailService.SendOtpEmailAsync(resendOtpDto.Email, otpCode);

      return Ok(new ApiResponse(200, "A new OTP has been sent"));
    }

    [AllowAnonymous]
    [HttpGet("emailexists")]
    public async Task<ActionResult<bool>> CheckEmailExists([FromQuery] string email)
    {
        // Check both Identity and Store databases
        var existsInIdentity = await _userManager.FindByEmailAsync(email) != null;
        var existsInStore = await _unitOfWork.Users.EmailExistsAsync(email);
 
        return existsInIdentity || existsInStore;
    }

  [AllowAnonymous]
  [HttpGet("phoneexists")]
    public async Task<ActionResult<bool>> CheckPhoneExists([FromQuery] string phoneNumber)
    {
        return await _userManager.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
    }

  [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<AccountProfileDto>> GetProfile()
    {
    var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email!);

        if (user == null)
        {
            return NotFound(new ApiResponse(404, "User not found"));
        }

      return Ok(new AccountProfileDto
  {
    DisplayName = user.DisplayName,
      Email = user.Email!,
            PhoneNumber = user.PhoneNumber
        });
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<AccountProfileDto>> UpdateProfile(UpdateAccountProfileDto updateProfileDto)
    {
     var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email!);

 if (user == null)
        {
     return NotFound(new ApiResponse(404, "User not found"));
      }

        // Check if phone number is already in use by another user
        if (!string.IsNullOrEmpty(updateProfileDto.PhoneNumber) && 
    updateProfileDto.PhoneNumber != user.PhoneNumber)
        {
            var phoneExists = await _userManager.Users
     .AnyAsync(u => u.PhoneNumber == updateProfileDto.PhoneNumber);
            
 if (phoneExists)
            {
         return BadRequest(new ApiResponse(400, "Phone number is already in use"));
     }
      }

  user.DisplayName = updateProfileDto.DisplayName;
        user.PhoneNumber = updateProfileDto.PhoneNumber;

     var result = await _userManager.UpdateAsync(user);

   if (!result.Succeeded)
 {
    return BadRequest(new ApiResponse(400, "Failed to update profile"));
  }

     return Ok(new AccountProfileDto
        {
     DisplayName = user.DisplayName,
 Email = user.Email!,
        PhoneNumber = user.PhoneNumber
});
    }

    [Authorize]
    [HttpGet("addresses")]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> GetUserAddresses()
    {
        try
        {
            var userId = GetCurrentUserId();
            var addresses = await _userAddressService.GetUserAddressesAsync(userId);
            return Ok(addresses);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    [Authorize]
    [HttpPost("addresses")]
    public async Task<ActionResult<AddressDto>> CreateAddress(CreateAddressDto createAddressDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var addressDto = await _userAddressService.CreateAddressAsync(userId, createAddressDto);
            return CreatedAtAction(nameof(GetUserAddresses), new { id = addressDto.Id }, addressDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    [Authorize]
    [HttpPut("addresses/{id}")]
    public async Task<ActionResult<AddressDto>> UpdateAddress(int id, UpdateAddressDto updateAddressDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var addressDto = await _userAddressService.UpdateAddressAsync(userId, id, updateAddressDto);
            return Ok(addressDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
        }
    }

    [Authorize]
    [HttpDelete("addresses/{id}")]
    public async Task<ActionResult> DeleteAddress(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _userAddressService.DeleteAddressAsync(userId, id);
            return Ok(new ApiResponse(200, "Address deleted successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(404, ex.Message));
        }
    }
}
