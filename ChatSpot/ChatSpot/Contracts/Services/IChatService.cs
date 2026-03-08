using ChatSpot.Dtos;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Dtos.Responses;
using ChatSpot.ResourceParameters;

namespace ChatSpot.Contracts.Services;

public interface IChatService
{
    Task<MessageToReturnDto> SendMessage(MessageForSending messageForSending, string currentUser);

    Task<PagedResult<ConversationToReturnDto>> GetAllConversations(BaseResourceParameter resourceParameter,
        string userId);
}