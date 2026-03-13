using ChatSpot.Dtos.Outgoing;
using ChatSpot.Dtos.Responses;
using ChatSpot.ResourceParameters;

namespace ChatSpot.Contracts.Services;

public interface IUserService
{
    public Task<PagedResult<UserDto>> SearchUsers(BaseResourceParameter resourceParameter , string excludedUser);
}