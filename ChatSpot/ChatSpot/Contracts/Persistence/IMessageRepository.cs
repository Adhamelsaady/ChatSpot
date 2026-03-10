using ChatSpot.Dtos.Responses;
using ChatSpot.Models.NoSQL;
using ChatSpot.ResourceParameters;

namespace ChatSpot.Contracts.Persistence;

public interface IMessageRepository
{
    Task<MessageDocument?> GetMessageByIdAsync(string id);
    Task<MessageDocument> CreateMessageAsync(MessageDocument message);
    Task<PagedResult<MessageDocument>> GetMessagesOfConversationAsync(BaseResourceParameter resourceParameter,
        string conversationId);
    Task<PagedResult<MessageDocument>> GetMessagesOfGroup(BaseResourceParameter resourceParameter
        , string groupId);
}