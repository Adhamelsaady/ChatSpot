using ChatSpot.Dtos.Responses;
using ChatSpot.Models.NoSQL;
using ChatSpot.ResourceParameters;

namespace ChatSpot.Contracts.Persistence;

public interface IConversationRepository
{
    Task<ConversationDocument> UpsertAsync(string senderId , string recipientId, string messageContent);
    Task<ConversationDocument?> GetByParticipantsAsync(string userId1, string userId2);
    
    Task<PagedResult<ConversationDocument>> GetAllConversations(BaseResourceParameter resourceParameter, string userId);
}