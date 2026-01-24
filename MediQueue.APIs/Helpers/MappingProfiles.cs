using AutoMapper;
using MediQueue.Core.DTOs;
using MediQueue.Core.Entities.Identity;

namespace MediQueue.APIs.Helpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Identity mappings
        CreateMap<Address, AddressDto>().ReverseMap();
        CreateMap<CreateAddressDto, Address>();
        CreateMap<UpdateAddressDto, Address>();
      
        // No additional mappings needed for clinic system
        // DTOs are manually mapped in services for better control
    }
}
