using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Api.IntegrationTests.TestFramework;
using Oma.WndwCtrl.Core.Model.Commands;
using Oma.WndwCtrl.Core.Model.Transformations;

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
        using HttpRequestMessage httpRequestMessage = ConstructCommandHttpRequestMessage(Payloads.NoOpCommand);
        using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

        httpResponse.Should().Be200Ok();
    }

    [Fact]
    public async Task ShouldAcceptSingleParserTransformation()
    {
        using HttpRequestMessage httpRequestMessage = ConstructCommandHttpRequestMessage(Payloads.ParserTransformationCommand);
        using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

        httpResponse.Should().Be200Ok().And
            .Satisfy<TransformationOutcome<BaseCommand>>(response =>
            {
                response.Success.Should().BeTrue();
                response.Outcome.Should().NotBeNull();
                response.Outcome!.Retries.Should().Be(1);
                response.Outcome!.Transformations.Should().HaveCount(1);
                response.Outcome!.Transformations.First().Should().BeOfType<ParserTransformation>();
            });
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(1337)]
    [InlineData(1337.0f)]
    [InlineData(1337.0d)]
    [InlineData("some-plain-string")]
    public async Task ShouldRejectInvalidPayloadsWith400(object payload, bool isJson = false)
    {
        using HttpRequestMessage httpRequestMessage = ConstructCommandHttpRequestMessage(payload.ToString()!, isJson);
        using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

        httpResponse.Should().Be400BadRequest();
    }

    private static HttpRequestMessage ConstructCommandHttpRequestMessage(string payload, bool isJson = true)
    {
        ArgumentNullException.ThrowIfNull(payload);

        HttpContent? json = null;
        HttpRequestMessage? httpRequestMessage = null;

        try
        {
            object? inputValue = isJson
                ? JsonSerializer.Deserialize<object>(payload)
                : payload;
            
            json = JsonContent.Create(inputValue);
            httpRequestMessage = new(HttpMethod.Post, CommandRoute);
            httpRequestMessage.Content = json;
            return httpRequestMessage;
        }
        catch
        {
            json?.Dispose();
            httpRequestMessage?.Dispose();
            throw;
        }
    }
    
    public void Dispose()
        => _httpClient.Dispose();
}