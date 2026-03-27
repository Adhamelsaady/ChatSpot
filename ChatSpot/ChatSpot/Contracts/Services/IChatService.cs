using ChatSpot.Dtos;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Dtos.Responses;
using ChatSpot.ResourceParameters;

namespace ChatSpot.Contracts.Services;

public interface IChatService
{
    Task<MessageToReturnDto> SendMessageAsync(MessageForSending messageForSending, string currentUser  ,string currentUserName,string conversationId);

    Task<PagedResult<ConversationToReturnDto>> GetAllConversationsAsync(BaseResourceParameter resourceParameter,
        string userId);

    Task<PagedResult<MessageToReturnDto>> GetMessagesOfConversationAsync(BaseResourceParameter resourceParameter,
        string conversationId , string userId);

    Task<BaseResponse> DeleteMessageAsync(string messageId, string userId);
    Task<string> CreateConversationAsync(string user1Id, string user2Id);
}