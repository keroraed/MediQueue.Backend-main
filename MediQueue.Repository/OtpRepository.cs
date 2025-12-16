using MediQueue.Core.Entities.Identity;
using MediQueue.Core.Enums;
using MediQueue.Core.Repositories;
using MediQueue.Repository.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.Repository;

public class OtpRepository : IOtpRepository
{
    private readonly AppIdentityDbContext _context;

    public OtpRepository(AppIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Otp> GetOtpByEmailAsync(string email, OtpPurpose purpose)
    {
        return await _context.Otps
            .Where(o => o.Email == email && o.Purpose == purpose && !o.IsUsed && o.ExpirationDate > DateTime.UtcNow)
            .OrderByDescending(o => o.ExpirationDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Otp> GetOtpByResetTokenAsync(string resetToken)
    {
        return await _context.Otps
            .Where(o => o.ResetToken == resetToken && o.ResetTokenExpiration > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    public async Task AddOtpAsync(Otp otp)
    {
        await _context.Otps.AddAsync(otp);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateOtpAsync(Otp otp)
    {
        _context.Otps.Update(otp);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOtpAsync(Otp otp)
    {
        _context.Otps.Remove(otp);
        await _context.SaveChangesAsync();
    }

    public async Task InvalidateAllOtpsByEmailAsync(string email, OtpPurpose purpose)
    {
        var otps = await _context.Otps
            .Where(o => o.Email == email && o.Purpose == purpose && !o.IsUsed)
            .ToListAsync();

        foreach (var otp in otps)
        {
            otp.IsUsed = true;
        }

        await _context.SaveChangesAsync();
    }
}
