using System.ComponentModel.DataAnnotations;

namespace ChatSpot.Dtos.Ingoing;

public class GroupToCreateDto
{
    [Required]
    public string Name { get; set; }

    public string Description { get; set; } = string.Empty;

    public List<string> Members { get; set; } = new List<string>();
}