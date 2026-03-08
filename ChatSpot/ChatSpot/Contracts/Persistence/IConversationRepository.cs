using ChatSpot.Models.NoSQL;

namespace ChatSpot.Contracts.Persistence;

public interface IConversationRepository
{
    Task<ConversationDocument> UpsertAsync(string senderId , string recipientId, string messageContent);
    Task<ConversationDocument?> GetByParticipantsAsync(string userId1, string userId2);
}