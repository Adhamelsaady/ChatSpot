namespace ChatSpot.Dtos.Outgoing;

public class ConversationToReturnDto
{
    public string Id { get; set; }
    public UserDto User { get; set; }
    public string LastMessage { get; set; }
    
    DateTime LastMessageDate { get; set; }
    public int UnreadMessagesCount { get; set; }
}