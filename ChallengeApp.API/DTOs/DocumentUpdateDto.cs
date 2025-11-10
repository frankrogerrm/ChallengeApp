using System.ComponentModel.DataAnnotations;
using ChallengeApp.API.Enums;

namespace ChallengeApp.API.DTOs;

public class DocumentUpdateDto
{
    [Required]
    public required string Title { get; set; }

    [Required]
    [RegularExpression(AccessTypes.ValidationPattern, ErrorMessage = AccessTypes.ValidationMessage)]
    public required string AccessType { get; set; }

    public List<TagDto> Tags { get; set; } = new();
}
