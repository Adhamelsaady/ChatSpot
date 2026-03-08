using ChatSpot.Models.NoSQL;

namespace ChatSpot.Contracts.Persistence;

public interface IMessageRepository
{
    Task<MessageDocument?> GetMessageByIdAsync(string id);
    Task<MessageDocument> CreateMessageAsync(MessageDocument message);
}