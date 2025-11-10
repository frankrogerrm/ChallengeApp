using System.ComponentModel.DataAnnotations;

public class DocumentShareDto
{
     [Required]
    public List<string> Emails { get; set; } = new();

    [Required]
    [RegularExpression("Read|Write", ErrorMessage = "Permission must be 'Read' or 'Write'.")]
    public string Permission { get; set; }
}