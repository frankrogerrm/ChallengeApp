using System.ComponentModel.DataAnnotations;
using Xunit;

public class DocumentShareDtoTests
{
    [Fact]
    public void ValidShareDto_ShouldPass()
    {
        var dto = new DocumentShareDto
        {
            Emails = new List<string> { "user@company.com" },
            Permission = "Read"
        };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        Assert.True(Validator.TryValidateObject(dto, context, results, true));
    }

    [Fact]
    public void InvalidPermission_ShouldFail()
    {
        var dto = new DocumentShareDto
        {
            Emails = new List<string> { "user@company.com" },
            Permission = "Execute"
        };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(dto, context, results, true));
    }
}
