using AutoMapper;
using ChatSpot.Contracts.Persistence;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Dtos.Responses;
using ChatSpot.Models.SQL;
using ChatSpot.ResourceParameters;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;

namespace ChatSpot.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Cloudinary _cloudinary;

    public UserService(
        IUserRepository userRepository,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        Cloudinary cloudinary)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _userManager = userManager;
        _cloudinary = cloudinary;
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

    public async Task<MyProfileDto?> GetMyProfile(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        return new MyProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio,
            ProfilePicture = user.ProfilePicture
        };
    }

    public async Task<UserDto?> GetUserById(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;
        return _mapper.Map<UserDto>(user);
    }

    public async Task<BaseResponse> UpdateProfile(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new BaseResponse { IsSuccess = false, Message = "User not found." };

        if (dto.FirstName != null) user.FirstName = dto.FirstName.Trim();
        if (dto.LastName  != null) user.LastName  = dto.LastName.Trim();
        if (dto.Bio       != null) user.Bio        = dto.Bio.Trim();

        if (dto.ProfilePicture != null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(dto.ProfilePicture.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                return new BaseResponse { IsSuccess = false, Message = "Only image files are allowed." };
            if (dto.ProfilePicture.Length > 5 * 1024 * 1024)
                return new BaseResponse { IsSuccess = false, Message = "Profile picture must be under 5 MB." };

            using var stream = dto.ProfilePicture.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(dto.ProfilePicture.FileName, stream),
                Folder = "chatspot/profile-pictures",
                Transformation = new Transformation().Width(300).Height(300).Crop("fill").Gravity("face")
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                return new BaseResponse { IsSuccess = false, Message = "Failed to upload profile picture." };

            user.ProfilePicture = uploadResult.SecureUrl.ToString();
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return new BaseResponse { IsSuccess = false, Message = result.Errors.First().Description };

        return new BaseResponse { IsSuccess = true, Message = "Profile updated successfully.", Data = user.ProfilePicture };
    }

    public async Task<BaseResponse> ChangePassword(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new BaseResponse { IsSuccess = false, Message = "User not found." };

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return new BaseResponse { IsSuccess = false, Message = result.Errors.First().Description };

        return new BaseResponse { IsSuccess = true, Message = "Password changed successfully." };
    }
}