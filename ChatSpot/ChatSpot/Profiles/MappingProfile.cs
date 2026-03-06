using AutoMapper;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Models.SQL;

namespace ChatSpot.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser , RegisterDto>();
    }
}