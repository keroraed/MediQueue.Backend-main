using MediQueue.Core.DTOs;

namespace MediQueue.Core.Services;

public interface IUserAddressService
{
    Task<IReadOnlyList<AddressDto>> GetUserAddressesAsync(string userId);
    Task<AddressDto> CreateAddressAsync(string userId, CreateAddressDto dto);
    Task<AddressDto> UpdateAddressAsync(string userId, int addressId, UpdateAddressDto dto);
    Task DeleteAddressAsync(string userId, int addressId);
}
