using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Oma.WndwCtrl.Api.IntegrationTests.TestFramework;

namespace Oma.WndwCtrl.Api.IntegrationTests.Endpoints.TestController;

public sealed partial class CommandDeserialization : IDisposable
{
    private const string CommandRoute = $"{Controllers.TestController.BaseRoute}/{Controllers.TestController.CommandRoute}";

    private readonly HttpClient _httpClient;
    private readonly CancellationToken _cancelToken;
    
    public CommandDeserialization(ApiAssemblyFixture apiAssemblyFixture)
    {
        _cancelToken = TestContext.Current.CancellationToken;
        
        _httpClient = apiAssemblyFixture.CreateClient();
    }

    [Fact]
    public async Task ShouldAcceptCommandWithoutTransformation()
    {
        using HttpContent json = JsonContent.Create(JsonSerializer.Deserialize<object>(Payloads.NoOpCommand));
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, CommandRoute)
        {
            Content = json,
        };
        using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

        httpResponse.Should().Be200Ok();
    }
    
    [Theory]
    [InlineData("some-plain-string")]
    public async Task ShouldRejectInvalidPayloadsWith400(object payload)
    {
        
    }

    public void Dispose()
        => _httpClient.Dispose();
}