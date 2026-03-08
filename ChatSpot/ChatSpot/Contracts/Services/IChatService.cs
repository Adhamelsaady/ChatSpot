using ChatSpot.Dtos;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;

namespace ChatSpot.Contracts.Services;

public interface IChatService
{
    Task<MessageToReturnDto> SendMessage(MessageForSending messageForSending, string currentUser);
}