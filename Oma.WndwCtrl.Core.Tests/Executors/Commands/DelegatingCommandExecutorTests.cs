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
    private readonly DelegatingCommandExecutor _instance;
    
    public DelegatingCommandExecutorTests()
    {
        var commandMock = Substitute.For<ICommand>();
        var loggerMock = Substitute.For<ILogger<DelegatingCommandExecutor>>();
        var executorMock = Substitute.For<ICommandExecutor>();

        var outcome = new CommandOutcome()
        {       
            OutcomeRaw = "This is a test outcome."
        };
        
        Either<CommandError, (CommandState, CommandOutcome)> test =
            Either<CommandError, (CommandState, CommandOutcome)>.Right((new(commandMock), outcome));

        MyState<CommandState, CommandOutcome> help = state => test.AsTask(); 
        
        executorMock.Handles(Arg.Any<ICommand>()).Returns(true);
        executorMock.ExecuteAsync(Arg.Any<ICommand>()).Returns(Task.FromResult(help));
        
        _instance = new DelegatingCommandExecutor(loggerMock, [executorMock]);
    }

    [Fact]
    public async Task ShouldSuccessfullyExecute()
    {
        ICommand commandMock = Substitute.For<ICommand>();
        var result = await _instance.ExecuteAsync(commandMock);
    }
}