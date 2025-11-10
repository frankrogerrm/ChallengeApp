using System.ComponentModel.DataAnnotations;
using ChallengeApp.API.Enums;

namespace ChallengeApp.API.DTOs;

public class DocumentUploadDto
{
    [Required]
    public required string Title { get; set; }

    [Required]
    [RegularExpression(AccessTypes.ValidationPattern, ErrorMessage = AccessTypes.ValidationMessage)]
    public required string AccessType { get; set; }

    [Required]
    public required string Tags { get; set; }

    [Required]
    public required IFormFile File { get; set; }
}
