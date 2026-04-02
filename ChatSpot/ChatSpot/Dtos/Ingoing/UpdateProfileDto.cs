namespace ChatSpot.Dtos.Ingoing;

public class UpdateProfileDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public IFormFile? ProfilePicture { get; set; }
}
