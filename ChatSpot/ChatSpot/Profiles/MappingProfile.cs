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
        CreateMap<ApplicationUser , UserDto>()
            .ForMember(dest => dest.Status , 
                opt => opt.MapFrom(src => src.isOnline ? "Online" : "Offline"));
        CreateMap<GroupToCreateDto, Group>()
            .ForMember(dest => dest.Members, opt => opt.MapFrom(src => 
                src.Members.Select(id => new GroupMember { UserId = id })));
        CreateMap<GroupMemberDto, GroupMember>().ReverseMap();
        CreateMap<Group, GroupToReturnDto>().ForMember(dest => dest.GroupMemberDtos, 
            opt => opt.MapFrom(src => src.Members));
        CreateMap<GroupMember, GroupMemberToReturnDto>()
            .ForMember(dest => dest.UserName, 
                opt => opt.MapFrom(src => src.User.UserName))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.JoinedAt));
    }
}