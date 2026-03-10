using ChatSpot.Models.NoSQL;

namespace ChatSpot.Contracts.Persistence;

public interface IGroupMetaDataRepository
{
    Task<GroupChatMetaDocument?> GetByGroupIdAsync(string groupId);
    Task<GroupChatMetaDocument> GetOrCreateAsync(Guid groupId);
    Task MarkGroupMessagesAsRead(string groupId, string userId);
    Task<IList<GroupChatMetaDocument>> GetByGroupIdsAsync(IEnumerable<string> groupIds);
}