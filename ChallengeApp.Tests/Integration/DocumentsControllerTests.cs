using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Xunit;

public class DocumentsControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;

    public DocumentsControllerTests(ApiTestFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UploadDocument_WithoutTags_ShouldReturnBadRequest()
    {
        var token = await AuthHelper.GetTokenAsync(_client, "contributor@company.com", "Contributor@123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent
        {
            { new StringContent("Test Title"), "title" },
            { new StringContent("Public"), "accessType" },
            { new StringContent("[]"), "tags" },
            { new ByteArrayContent(Encoding.UTF8.GetBytes("dummy")), "file", "test.pdf" }
        };

        var response = await _client.PostAsync("/api/v1/documents/upload", content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
