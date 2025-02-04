using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Api.IntegrationTests.TestFramework;
using Oma.WndwCtrl.CliOutputParser.Interfaces;

namespace Oma.WndwCtrl.Api.IntegrationTests.Endpoints.TestController;

public sealed partial class TransformationChainTests(
  MockedCommandExecutorApiFixture mockedCommandExecutorApiFixture
)
  : ApiFixtureTestBase<MockedCommandExecutorApiFixture>(mockedCommandExecutorApiFixture, CommandRoute)
{
  private const string CommandRoute =
    $"{Controllers.TestController.BaseRoute}/{Controllers.TestController.CommandRoute}";

  [Fact]
  public async Task ShouldProcessEmptyCommandAndTransformations()
  {
    using HttpRequestMessage httpRequestMessage =
      ConstructCommandHttpRequestMessage(Payloads.DummyCommandNoTransformation);

    using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

    httpResponse.Should().Be200Ok();
  }

  [Fact]
  public async Task ShouldProcessPingResultInOneTransformation()
  {
    string payload = Payloads.PingResultOneTransformation;
    using HttpRequestMessage httpRequestMessage = ConstructCommandHttpRequestMessage(payload, isJson: true);
    using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

    const double expected = 7.5;

    httpResponse.Should().Be200Ok().And.Satisfy<TransformationOutcome<ParserResult>>(
      response => { AssertParserResultOneNumber(response, expected); }
    );
  }

  [Fact]
  public async Task ShouldProcessPingResultInMultipleTransformations()
  {
    string payload = Payloads.PingResultMultipleTransformations;
    using HttpRequestMessage httpRequestMessage = ConstructCommandHttpRequestMessage(payload, isJson: true);
    using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

    const double expected = 7.5;

    httpResponse.Should().Be200Ok().And.Satisfy<TransformationOutcome<ParserResult>>(
      response => { AssertParserResultOneNumber(response, expected); }
    );
  }

  [Fact]
  public async Task ShouldUnwrapParserTransformationResult()
  {
    string payload = Payloads.PingResultOneTransformation;
    using HttpRequestMessage httpRequestMessage = ConstructCommandHttpRequestMessage(payload, isJson: true);
    using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

    const double expected = 7.5;
  }

  [Fact]
  public async Task ShouldReturnProblemDetailsOnEmptyValueAggregation()
  {
    string payload = Payloads.PingTimeoutResultOneTransformation;
    using HttpRequestMessage httpRequestMessage = ConstructCommandHttpRequestMessage(payload, isJson: true);
    using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

    httpResponse.Should().Be500InternalServerError().And.Satisfy<ProblemDetails>(
      response =>
      {
        response.Detail.Should()
          .Be(
            "Transformation expected at least one value to aggregate, but found none."
          );
      }
    );
  }

  [Fact]
  public async Task ShouldReturn500OnEmptyValueAggregation()
  {
    string payload = Payloads.PingTimeoutResultOneTransformation;
    using HttpRequestMessage httpRequestMessage = ConstructCommandHttpRequestMessage(payload, isJson: true);
    using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

    httpResponse.Should().Be500InternalServerError();
  }

  [Fact]
  public async Task ShouldReturnJsonOnEmptyValueAggregation()
  {
    string payload = Payloads.PingTimeoutResultOneTransformation;
    using HttpRequestMessage httpRequestMessage = ConstructCommandHttpRequestMessage(payload, isJson: true);
    using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

    httpResponse.Should().HaveHeader("Content-Type").And.Match("application/json*");
  }

  private static void AssertParserResultOneNumber(
    TransformationOutcome<ParserResult> response,
    double expectedValue
  )
  {
    response.Success.Should().BeTrue();
    response.Outcome.Should().NotBeNull();
    response.Outcome!.First().Should().BeAssignableTo<JsonElement>();
    JsonElement jsonElement = (JsonElement)response.Outcome!.First();
    double num = jsonElement.GetDouble();
    num.Should().Be(expectedValue);
  }
}