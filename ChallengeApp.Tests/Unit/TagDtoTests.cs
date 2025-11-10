using System.ComponentModel.DataAnnotations;

public class TagDtoTests
{
    [Theory]
    [InlineData("ValidTag")]
    [InlineData("tag-123")]
    public void ValidTags_ShouldPass(string name)
    {
        var dto = new TagDto { Name = name };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        Assert.True(Validator.TryValidateObject(dto, context, results, true));
    }

    [Theory]
    [InlineData("")]
    [InlineData("!invalid")]
    public void InvalidTags_ShouldFail(string name)
    {
        var dto = new TagDto { Name = name };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(dto, context, results, true));
    }
}
