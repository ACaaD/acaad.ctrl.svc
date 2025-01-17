using FluentAssertions;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Api.IntegrationTests.TestFramework;
using Oma.WndwCtrl.Core.Model.Transformations;

namespace Oma.WndwCtrl.Api.IntegrationTests.Endpoints.TestController;

public sealed partial class CommandDeserializationTests(
  MockedFlowExecutorApiFixture mockedFlowExecutorApiFixture
)
  : ApiFixtureTestBase<MockedFlowExecutorApiFixture>(
    mockedFlowExecutorApiFixture,
    CommandRoute
  )
{
  private const string CommandRoute =
    $"{Controllers.TestController.BaseRoute}/{Controllers.TestController.CommandRoute}";

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
    using HttpRequestMessage httpRequestMessage =
      ConstructCommandHttpRequestMessage(Payloads.ParserTransformationCommand);

    using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

    httpResponse.Should().Be200Ok().And
      .Satisfy<TransformationOutcome<ICommand>>(
        response =>
        {
          response.Success.Should().BeTrue();
          response.Outcome.Should().NotBeNull();
          response.Outcome!.Retries.Should().Be(expected: 1);
          response.Outcome!.Transformations.Should().HaveCount(expected: 1);
          response.Outcome!.Transformations.First().Should().BeOfType<ParserTransformation>();
        }
      );
  }

  [Theory]
  [InlineData(true)]
  [InlineData(1337)]
  [InlineData(1337.0f)]
  [InlineData(1337.0d)]
  [InlineData("some-plain-string")]
  public async Task ShouldRejectInvalidPayloadsWith400(object payload, bool isJson = false)
  {
    using HttpRequestMessage httpRequestMessage =
      ConstructCommandHttpRequestMessage(payload.ToString()!, isJson);

    using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

    httpResponse.Should().Be400BadRequest();
  }
}