using AutoMapper;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Models.NoSQL;
using ChatSpot.Models.SQL;

namespace ChatSpot.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterDto , ApplicationUser>();
        CreateMap<MessageForSending , MessageDocument>();
        CreateMap<MessageDocument, MessageToReturnDto>();
        CreateMap<ApplicationUser , UserDto>();
    }
}