using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using ChatSpot.Contracts.Persistence;
using ChatSpot.Contracts.Services;
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

    public async Task<PagedResult<ConversationToReturnDto>> GetAllConversations(BaseResourceParameter resourceParameter , string userId)
    {
        var conversations = await _conversationRepository.GetAllConversations(resourceParameter , userId);
        var result = new  PagedResult<ConversationToReturnDto>();
        foreach (var conversation in conversations.Items)
        {
            var user = await _userRepository.GetByIdAsync(conversation.Participants.First(p => p != userId));
            var unreadCount = conversation.UnreadCount[userId];
            result.Items.Add(new ConversationToReturnDto()
            {
                Id =  conversation.Id,
                User = _mapper.Map<UserDto>(user),
                LastMessage = conversation.LastMessage,
                UnreadMessagesCount =  unreadCount,
            });
        }
        result.TotalCount = conversations.TotalCount;
        result.PageSize = resourceParameter.PageSize;
        result.PageNumber = resourceParameter.PageNumber;
        return result;
    }
    
    public async Task<MessageToReturnDto> SendMessage(MessageForSending messageForSending , string currentUser)
    {
        var receiver = await _userRepository.GetByIdAsync(messageForSending.ReceiverId);
        if (receiver == null)
        {
            return new MessageToReturnDto() {IsSuccess =  false , Message = "User not found"}; 
        }

        var messageDocument = _mapper.Map<MessageDocument>(messageForSending);
        string? replyPreview = null;
        if(!string.IsNullOrEmpty(messageForSending.ReplyToId))
        {
            var messageToReply = await _messageRepository.GetMessageByIdAsync(messageForSending.ReplyToId);
            if (messageToReply.IsDeleted) replyPreview = "Deleted Message";
            else replyPreview = messageToReply.Content[..Math.Min(60 , messageToReply.Content.Length)];
        }
        // map between message to create dto and message
        messageDocument.ReplyToPreview = replyPreview;
        messageDocument.SenderId = currentUser;
        messageDocument.Timestamp = DateTime.UtcNow;
        var message = await _messageRepository.CreateMessageAsync(messageDocument);
        await _conversationRepository.UpsertAsync(messageDocument.SenderId, messageDocument.ReceiverId,
            messageDocument.Content);
        var result = _mapper.Map<MessageToReturnDto>(message);
       await ChatHub.SendToUserAsync(_chatHub , messageDocument.ReceiverId , "ReceiveMessage" , result);
        result.IsSuccess = true;
        result.Message = "Message sent";
        return result;
    }
}