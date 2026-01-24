using AutoMapper;
using MediQueue.Core.DTOs;
using MediQueue.Core.Entities.Identity;
using MediQueue.Core.Services;
using MediQueue.Repository.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.Service;

public class UserAddressService : IUserAddressService
{
    private readonly AppIdentityDbContext _identityContext;
    private readonly IMapper _mapper;

    public UserAddressService(AppIdentityDbContext identityContext, IMapper mapper)
    {
        _identityContext = identityContext;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<AddressDto>> GetUserAddressesAsync(string userId)
    {
        var user = await _identityContext.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        return _mapper.Map<List<AddressDto>>(user.Addresses);
    }

    public async Task<AddressDto> CreateAddressAsync(string userId, CreateAddressDto dto)
    {
        var user = await _identityContext.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        var address = _mapper.Map<Address>(dto);
        address.AppUserId = userId;

        user.Addresses.Add(address);
        await _identityContext.SaveChangesAsync();

        return _mapper.Map<AddressDto>(address);
    }

    public async Task<AddressDto> UpdateAddressAsync(string userId, int addressId, UpdateAddressDto dto)
    {
        var user = await _identityContext.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        var address = user.Addresses.FirstOrDefault(a => a.Id == addressId);

        if (address == null)
            throw new KeyNotFoundException($"Address with ID {addressId} not found for this user");

        _mapper.Map(dto, address);
        await _identityContext.SaveChangesAsync();

        return _mapper.Map<AddressDto>(address);
    }

    public async Task DeleteAddressAsync(string userId, int addressId)
    {
        var user = await _identityContext.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        var address = user.Addresses.FirstOrDefault(a => a.Id == addressId);

        if (address == null)
            throw new KeyNotFoundException($"Address with ID {addressId} not found for this user");

        user.Addresses.Remove(address);
        await _identityContext.SaveChangesAsync();
    }
}
