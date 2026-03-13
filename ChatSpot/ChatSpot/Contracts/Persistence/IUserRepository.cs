using ChatSpot.Dtos.Responses;
using ChatSpot.Models.SQL;
using ChatSpot.ResourceParameters;

namespace ChatSpot.Contracts.Persistence;

public interface IUserRepository
{
    public Task<PagedResult<ApplicationUser>> SearchUsers(BaseResourceParameter resourceParameter , string excludedUser);
}