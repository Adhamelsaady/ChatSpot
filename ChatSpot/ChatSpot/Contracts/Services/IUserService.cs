using ChatSpot.Dtos;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Dtos.Responses;
using ChatSpot.ResourceParameters;

namespace ChatSpot.Contracts.Services;

public interface IUserService
{
    public Task<PagedResult<UserDto>> SearchUsers(BaseResourceParameter resourceParameter, string excludedUser);
    public Task<MyProfileDto?> GetMyProfile(string userId);
    public Task<UserDto?> GetUserById(string userId);
    public Task<BaseResponse> UpdateProfile(string userId, UpdateProfileDto dto);
    public Task<BaseResponse> ChangePassword(string userId, ChangePasswordDto dto);
}