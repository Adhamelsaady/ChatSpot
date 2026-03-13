using AutoMapper;
using ChatSpot.Contracts.Persistence;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Dtos.Responses;
using ChatSpot.ResourceParameters;

namespace ChatSpot.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    public UserService(IUserRepository userRepository , IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserDto>> SearchUsers(BaseResourceParameter resourceParameter, string excludedUser)
    {
        var result = await _userRepository.SearchUsers(resourceParameter, excludedUser);
        var usersToReturn = _mapper.Map<List<UserDto>>(result.Items);
        return new PagedResult<UserDto>
        {
            TotalCount = result.TotalCount,
            Items = usersToReturn,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }
}