using FluentAssertions;
using LanguageExt;
using static LanguageExt.Prelude;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Executors.Commands;

namespace Oma.WndwCtrl.Core.Tests.Executors.Commands;

public class DelegatingCommandExecutorTests
{
    private const string _rawOutcome = "This is a test outcome.";
    private readonly DelegatingCommandExecutor _instance;
    
    public DelegatingCommandExecutorTests()
    {
        var loggerMock = Substitute.For<ILogger<DelegatingCommandExecutor>>();
        var executorMock = Substitute.For<ICommandExecutor>();

        var outcome = new CommandOutcome()
        {       
            OutcomeRaw = _rawOutcome
        };
        
        executorMock.Handles(Arg.Any<ICommand>()).Returns(true);
        executorMock.ExecuteAsync(Arg.Any<ICommand>()).Returns(Right(outcome));
        
        _instance = new(loggerMock, [executorMock]);
    }

    [Fact]
    public async Task ShouldSuccessfullyExecute()
    {
        ICommand commandMock = Substitute.For<ICommand>();
        var result = await _instance.ExecuteAsync(commandMock);
        
        result.IsRight.Should().BeTrue();

        result.Match(
            Right: val => val.OutcomeRaw.Should().Be(_rawOutcome),
            Left: val => val.Should().BeNull()
        );
    }
}