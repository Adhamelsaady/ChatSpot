namespace ChatSpot.Dtos.Outgoing;

public class UserDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Status { get; set; }
    public DateTime LastSeen {get; set;}
}