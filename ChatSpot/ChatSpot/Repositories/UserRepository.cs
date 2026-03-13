using ChatSpot.Contracts.Persistence;
using ChatSpot.Dtos.Responses;
using ChatSpot.Models.SQL;
using ChatSpot.ResourceParameters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatSpot.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ChatSpotDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(ChatSpotDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ApplicationUser>> SearchUsers(BaseResourceParameter resourceParameter,
        string excludedUser)
    {
        var normalized = resourceParameter.SearchQuery!.Trim().ToUpperInvariant();

        var ranked = _userManager.Users.Where(u => u.Id != excludedUser && u.EmailConfirmed  && u.NormalizedUserName!.Contains(normalized))
            .Select(u => new
            {
                User = u,
                Rank = u.NormalizedUserName == normalized ? 1
                    : u.NormalizedUserName!.StartsWith(normalized) ? 2
                    : 3
            })
            .OrderBy(x => x.Rank)
            .ThenBy(x => x.User.NormalizedUserName);
        var totalCount = await ranked.CountAsync();
        var items = await ranked
            .Skip((resourceParameter.PageNumber - 1) * resourceParameter.PageSize)
            .Take(resourceParameter.PageSize)
            .Select(x => x.User)
            .ToListAsync();

        return new PagedResult<ApplicationUser>()
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = resourceParameter.PageNumber,
            PageSize = resourceParameter.PageSize
        };
    }
}