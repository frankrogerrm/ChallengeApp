using System.ComponentModel.DataAnnotations;
using ChallengeApp.API.DTOs;
using Microsoft.AspNetCore.Http;
using Xunit;

public class DocumentUploadDtoTests
{
    [Fact]
    public void MissingTags_ShouldFail()
    {
        var dto = new DocumentUploadDto
        {
            Title = "Test",
            AccessType = "Public",
            Tags = "",
            File = new FormFile(Stream.Null, 0, 0, "file", "test.pdf")
        };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(dto, context, results, true));
    }
}
