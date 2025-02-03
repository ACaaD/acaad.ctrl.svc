using FluentAssertions;
using FluentAssertions.Execution;
using LanguageExt;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Executors.Commands;
using Oma.WndwCtrl.Core.Mocks;
using Oma.WndwCtrl.Core.Model.Commands;

namespace Oma.WndwCtrl.Core.IntegrationTests.Executors.Commands;

public sealed class CliCommandExecutorTests : IDisposable
{
  private const string DEFAULT_OUTPUT_TEXT = "Hello World!";

  private static readonly ILogger<CliCommandExecutor> _logger = Substitute.For<ILogger<CliCommandExecutor>>();

  private readonly CliCommandExecutor _instance = new(_logger);
  private readonly CancellationToken _xunitCancelToken = TestContext.Current.CancellationToken;

  private Either<FlowError, CommandOutcome>? _result;

  public void Dispose()
  {
    _result?.Dispose();
  }

  [Fact]
  public async Task SanityCheck()
  {
    CliCommand command = CreateCommand();

    Func<Task<Either<FlowError, CommandOutcome>>> act = async () =>
      await _instance.ExecuteAsync(command, _xunitCancelToken);

    await act.Should().NotThrowAsync();
  }

  [Fact]
  public async Task ShouldSetSuccessTrueIfExitCodeIsZero()
  {
    CliCommand command = CreateCommand();

    _result = await _instance.ExecuteAsync(command, _xunitCancelToken);

    SatisfiesRight(
      outcome => outcome.Success.Should().BeTrue()
    );
  }

  [Fact]
  public async Task ShouldSetSuccessFalseIfExitCodeIsNonZero()
  {
    CliCommand command = CreateCommand(exitCode: 1);

    _result = await _instance.ExecuteAsync(command, _xunitCancelToken);

    SatisfiesRight(
      outcome => outcome.Success.Should().BeFalse()
    );
  }

  [Fact]
  public async Task ShouldSetSuccessFalseIfStdErrIsWritten()
  {
    CliCommand command = CreateCommand(target: CliInvocationOptions.OutputTarget.StdErr);

    _result = await _instance.ExecuteAsync(command, _xunitCancelToken);

    SatisfiesRight(
      outcome => outcome.Success.Should().BeFalse()
    );
  }

  [Fact]
  public async Task ShouldReturnStdErrIfWrittenTo()
  {
    CliCommand command = CreateCommand(target: CliInvocationOptions.OutputTarget.StdErr);

    _result = await _instance.ExecuteAsync(command, _xunitCancelToken);

    SatisfiesRight(
      outcome => outcome.OutcomeRaw.Should().Be(DEFAULT_OUTPUT_TEXT)
    );
  }

  [Fact]
  public async Task ShouldHandleWritesToBothStreams()
  {
    CliCommand command = CreateCommand(
      target: CliInvocationOptions.OutputTarget.StdOut | CliInvocationOptions.OutputTarget.StdErr
    );

    _result = await _instance.ExecuteAsync(command, _xunitCancelToken);

    SatisfiesRight(
      outcome => outcome.Success.Should().BeFalse(),
      outcome => outcome.OutcomeRaw.Should().Be(DEFAULT_OUTPUT_TEXT)
    );
  }

  [Fact(Timeout = 2_000)]
  public async Task ShouldHandleAlternatingWrites()
  {
    string text = GetMultiLineText(lineCount: 10);

    CliCommand command = CreateCommand(
      text,
      CliInvocationOptions.OutputTarget.StdOut | CliInvocationOptions.OutputTarget.StdErr
    );

    _result = await _instance.ExecuteAsync(command, _xunitCancelToken);

    SatisfiesRight(
      outcome => outcome.Success.Should().BeFalse(),
      outcome => outcome.OutcomeRaw.Should().Be(text)
    );
  }

  [Fact(Timeout = 2_000)]
  public async Task ShouldNotTimeout()
  {
    string text = GetMultiLineText(lineCount: 1_000);

    CliCommand command = CreateCommand(
      text,
      CliInvocationOptions.OutputTarget.StdOut | CliInvocationOptions.OutputTarget.StdErr
    );

    _result = await _instance.ExecuteAsync(command, _xunitCancelToken);

    SatisfiesRight(
      outcome => outcome.Success.Should().BeFalse(),
      outcome => outcome.OutcomeRaw.Should().Be(text)
    );
  }

  private void SatisfiesRight(params Action<CommandOutcome>[] assertions)
  {
    using AssertionScope scope = new();

    _result.Should().NotBeNull();

    _result?.Match(
      Right: val =>
      {
        foreach (Action<CommandOutcome> assertion in assertions) assertion(val);
      },
      Left: val => val.Should().BeNull()
    );
  }

  private static string GetMultiLineText(int lineCount = 1_000)
  {
    string text = string.Join(
      Environment.NewLine,
      Enumerable.Range(start: 0, lineCount).Select(num => $"This is line {num}.")
    );

    return text;
  }

  private static CliCommand CreateCommand(
    string text = DEFAULT_OUTPUT_TEXT,
    CliInvocationOptions.OutputTarget target = CliInvocationOptions.OutputTarget.StdOut,
    int exitCode = 0
  )
  {
    CliCommand command = new()
    {
      FileName = "./Oma.WndwCtrl.Core.Mocks",
      Arguments = new CliInvocationOptions
      {
        Text = text,
        TargetStreams = target,
        ExitCode = exitCode,
      }.ToString(),
    };

    return command;
  }
}