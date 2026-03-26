using ChatSpot.Models.NoSQL;

namespace ChatSpot.Contracts.Persistence;

public interface IGroupMetaDataRepository
{
    Task<GroupChatMetaDocument?> GetByGroupIdAsync(string groupId);
    Task<GroupChatMetaDocument> GetOrCreateAsync(string groupId);
    Task MarkGroupMessagesAsRead(string groupId, string userId);
    Task<IList<GroupChatMetaDocument>> GetByGroupIdsAsync(IEnumerable<string> groupIds);
    Task<GroupChatMetaDocument> UpsertAsync(string groupId, string messageContent, string userId);
    Task IncrementUnreadAsync(string groupId, List<string> exceptUserIds);
    Task<GroupChatMetaDocument> UpdateLastMessage(string groupId, string messageId);
    Task UpdateGroupLastMessageSnapshotAsync(string groupId, string lastMessageId, string lastMessage, DateTime lastUpdated);
    Task ClearGroupLastMessageSnapshotAsync(string groupId);
}