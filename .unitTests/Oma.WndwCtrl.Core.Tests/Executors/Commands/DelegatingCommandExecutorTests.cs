using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Executors.Commands;
using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.Core.Tests.Executors.Commands;

public class DelegatingCommandExecutorTests
{
  private const string _rawOutcome = "This is a test outcome.";

  private readonly CancellationToken _cancelToken;
  private readonly ICommand _commandMock;
  private readonly ICommandExecutor _executorMock;

  private readonly DelegatingCommandExecutor _instance;

  public DelegatingCommandExecutorTests()
  {
    ILogger<DelegatingCommandExecutor>? loggerMock = Substitute.For<ILogger<DelegatingCommandExecutor>>();
    _executorMock = Substitute.For<ICommandExecutor>();

    CommandOutcome outcome = new()
    {
      OutcomeRaw = _rawOutcome,
    };

    _executorMock.Handles(Arg.Any<ICommand>()).Returns(returnThis: true);

    _executorMock.ExecuteAsync(Arg.Any<ICommand>(), Arg.Any<CancellationToken>())
      .Returns(Right(outcome));

    _commandMock = Substitute.For<ICommand>();

    _cancelToken = TestContext.Current.CancellationToken;

    _instance = new DelegatingCommandExecutor(loggerMock, [_executorMock,]);
  }

  [Fact]
  public async Task ShouldSuccessfullyExecute()
  {
    Either<FlowError, CommandOutcome> result = await _instance.ExecuteAsync(_commandMock, _cancelToken);

    result.IsRight.Should().BeTrue();

    result.Match(
      Right: val => val.OutcomeRaw.Should().Be(_rawOutcome),
      Left: val => val.Should().BeNull()
    );
  }

  [Fact]
  public async Task ShouldFailIfNoExecutorIsFound()
  {
    _executorMock.Handles(Arg.Any<ICommand>()).Returns(returnThis: false);

    Either<FlowError, CommandOutcome> result = await _instance.ExecuteAsync(_commandMock, _cancelToken);

    result.IsLeft.Should().BeTrue();

    // TODO:
    // result.Match(_ => { }, err => err.Message.Should().Contain("programming"));
  }

  [Fact]
  public async Task ShouldFailIfExecutorFails()
  {
    TechnicalError simulatedError = new("Simulated sub-command executor error", Code: 1337);

    _executorMock.ExecuteAsync(Arg.Any<ICommand>(), Arg.Any<CancellationToken>())
      .Returns(Left<FlowError>(simulatedError));

    Either<FlowError, CommandOutcome> result = await _instance.ExecuteAsync(_commandMock, _cancelToken);

    result.IsLeft.Should().BeTrue();
    result.Match(_ => { }, err => err.Should().BeOfType<FlowError>());
  }

  [Fact]
  public async Task ShouldPopulateMetadataOnError()
  {
    _executorMock.Handles(Arg.Any<ICommand>()).Returns(returnThis: false);

    Either<FlowError, CommandOutcome> result = await _instance.ExecuteAsync(_commandMock, _cancelToken);

    result.IsLeft.Should().BeTrue();

    // TODO

    // result.Match(_ => { },
    //     err =>
    //     {
    //         err.ExecutedRetries.Should<Option<int>>().Be(Option<int>.None, because: "No executor is run.");
    //         err.ExecutionDuration.Should<Option<TimeSpan>>().NotBe(Option<TimeSpan>.None, because: "execution duration is always populated");
    //     });
  }
}