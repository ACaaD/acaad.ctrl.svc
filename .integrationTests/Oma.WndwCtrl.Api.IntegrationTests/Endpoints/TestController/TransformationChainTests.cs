using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Api.IntegrationTests.TestFramework;
using Oma.WndwCtrl.CliOutputParser.Interfaces;

namespace Oma.WndwCtrl.Api.IntegrationTests.Endpoints.TestController;

internal class NestedProblemDetails : ProblemDetails
{
  public List<NestedProblemDetails> InnerErrors { get; set; } = new();
}

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

    const decimal expected = 7.5m;

    httpResponse.Should().Be200Ok().And.Satisfy<TransformationOutcome<decimal>>(
      response => { response.Outcome.Should().Be(expected); }
    );
  }

  [Fact]
  public async Task ShouldProcessPingResultInMultipleTransformations()
  {
    string payload = Payloads.PingResultMultipleTransformations;
    using HttpRequestMessage httpRequestMessage = ConstructCommandHttpRequestMessage(payload, isJson: true);
    using HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequestMessage, _cancelToken);

    const decimal expected = 7.5m;

    httpResponse.Should().Be200Ok().And.Satisfy<TransformationOutcome<decimal>>(
      response => { response.Outcome.Should().Be(expected); }
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

    httpResponse.Should().Be500InternalServerError().And.Satisfy<NestedProblemDetails>(
      response =>
      {
        response.Detail.Should()
          .Be(
            "The CLI parser was in an invalid state during the transformation and could not proceed."
          );

        response.InnerErrors.Should().HaveCount(expected: 1).And.Satisfy(
          pd => pd.Title ==
                "The CLI parser was in an invalid state during the transformation and could not proceed."
        ).And.Satisfy(
          pd => pd.Detail ==
                "Strict aggregation (function: 'Average') requires at least one value to be present, but the collection was empty. This could be caused by an invalid transformation or the input text was in an invalid/irregular format."
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