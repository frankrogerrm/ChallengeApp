using System.ComponentModel.DataAnnotations;

public class TagDto
{
    [Required]
    [StringLength(30, MinimumLength = 2)]
    [RegularExpression(@"^[a-zA-Z0-9\-_\s]+$", ErrorMessage = "Tag contains invalid characters.")]
    public string Name { get; set; }
}
