using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using ChatSpot.Contracts.Persistence;
using ChatSpot.Contracts.Services;
using ChatSpot.Dtos;
using ChatSpot.Dtos.Ingoing;
using ChatSpot.Dtos.Outgoing;
using ChatSpot.Dtos.Responses;
using ChatSpot.Hubs;
using ChatSpot.Models.NoSQL;
using ChatSpot.Models.SQL;
using ChatSpot.ResourceParameters;
using Microsoft.AspNetCore.SignalR;

namespace ChatSpot.Services;

public class ChatService : IChatService
{
    private readonly IBaseRepository<ApplicationUser> _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<ChatHub> _chatHub;

    public ChatService(IBaseRepository<ApplicationUser> userRepository
        , IMessageRepository messageRepository
        , IConversationRepository conversationRepository
        , IMapper mapper
        , IHubContext<ChatHub> chatHub)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _mapper = mapper;
        _chatHub = chatHub;
    }


    public async Task<PagedResult<ConversationToReturnDto>> GetAllConversationsAsync(
        BaseResourceParameter resourceParameter,
        string userId)
    {
        var conversations = await _conversationRepository.GetAllConversations(resourceParameter, userId);
        var result = new PagedResult<ConversationToReturnDto>();
        foreach (var conversation in conversations.Items)
        {
            var user = await _userRepository.GetByIdAsync(conversation.Participants.First(p => p != userId));
            var unreadCount = conversation.UnreadCount.ContainsKey(userId) ? conversation.UnreadCount[userId] : 0;
            result.Items.Add(new ConversationToReturnDto()
            {
                Id = conversation.Id,
                User = _mapper.Map<UserDto>(user),
                LastMessage = conversation.LastMessage,
                UnreadMessagesCount = unreadCount,
            });
        }

        result.TotalCount = conversations.TotalCount;
        result.PageSize = resourceParameter.PageSize;
        result.PageNumber = resourceParameter.PageNumber;
        return result;
    }

    public async Task<MessageToReturnDto> SendMessageAsync(MessageForSending messageForSending, string currentUser,
        string conversationId)
    {
        var messageDocument = _mapper.Map<MessageDocument>(messageForSending);
        string? replyPreview = null;
        if (!string.IsNullOrEmpty(messageForSending.ReplyToId))
        {
            var messageToReply = await _messageRepository.GetMessageByIdAsync(messageForSending.ReplyToId);
            if (messageToReply.IsDeleted) replyPreview = "Deleted Message";
            else replyPreview = messageToReply.Content[..Math.Min(60, messageToReply.Content.Length)];
        }

        messageDocument.ReceiverId = await GetReceiverIdAsync(conversationId, currentUser);
        messageDocument.ReplyToPreview = replyPreview;
        messageDocument.SenderId = currentUser;
        messageDocument.Timestamp = DateTime.UtcNow;
        await _conversationRepository.UpsertAsync(conversationId, messageDocument.SenderId, messageDocument.ReceiverId,
            messageDocument.Content);
        messageDocument.ConversationId = conversationId;
        var message = await _messageRepository.CreateMessageAsync(messageDocument);
        await _conversationRepository.UpdateLastMessage(conversationId, message.Id);
        var result = _mapper.Map<MessageToReturnDto>(message);
        result.IsSuccess = true;
        result.Message = "Message sent";
        await _chatHub.Clients.User(currentUser).SendAsync("ReceiveDirectMessage", conversationId, result);
        await _chatHub.Clients.User(messageDocument.ReceiverId).SendAsync("ReceiveDirectMessage", conversationId, result);
        return result;
    }

    public async Task<PagedResult<MessageToReturnDto>> GetMessagesOfConversationAsync(
        BaseResourceParameter resourceParameter,
        string conversationId, string userId)
    {
        var messages = await _messageRepository.GetMessagesOfConversationAsync(resourceParameter, conversationId);
        await _conversationRepository.MarkConversationAsRead(conversationId, userId);
        PagedResult<MessageToReturnDto> messagesToReturn = new PagedResult<MessageToReturnDto>()
        {
            Items = _mapper.Map<List<MessageToReturnDto>>(messages.Items),
            TotalCount = messages.TotalCount,
            PageNumber = messages.PageNumber,
            PageSize = messages.PageSize
        };
        return messagesToReturn;
    }

    public async Task<string> CreateConversationAsync(string user1Id, string user2Id)
    {
        var conv = await _conversationRepository.GetByParticipantsAsync(user1Id, user2Id);
        if (conv == null)
        {
            return (await _conversationRepository.CreateConversation(user1Id, user2Id)).Id;
        }

        return conv.Id;
    }

    public async Task<BaseResponse> DeleteMessageAsync(string messageId, string userId)
    {
        var message = await _messageRepository.GetMessageByIdAsync(messageId);
        if (message.SenderId != userId)
        {
            return new BaseResponse()
            {
                IsSuccess = false, Message = "UnAuthorized"
            };
        }

        if (message.IsDeleted == true)
        {
            return new BaseResponse()
            {
                IsSuccess = false, Message = "Already deleted"
            };
        }

        await _messageRepository.DeleteMessageAsync(messageId);
        var receiverId = await GetReceiverIdAsync(message.ConversationId, userId);
        await _chatHub.Clients.User(userId).SendAsync("DirectMessageDeleted", message.ConversationId, messageId);
        await _chatHub.Clients.User(receiverId).SendAsync("DirectMessageDeleted", message.ConversationId, messageId);

        return  new BaseResponse() {IsSuccess = true, Message = "Deleted Successfully"};
    }
    private async Task<string> GetReceiverIdAsync(string conversationId, string userId)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        return (conversation.Participants[0] == userId ? conversation.Participants[1] : conversation.Participants[0]);
    }
}