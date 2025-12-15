using Microsoft.EntityFrameworkCore;
using MediQueue.Core.Entities;
using MediQueue.Core.Repositories;
using MediQueue.Repository.Data;

namespace MediQueue.Repository;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(StoreContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
    return await _context.Users
   .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
}
