using FluentAssertions;
using FluentAssertions.Execution;
using LanguageExt;
using LanguageExt.Common;
using NSubstitute;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.Core.Executors.Transformers;
using Oma.WndwCtrl.Core.Model.Transformations;
using static LanguageExt.Prelude;
using ValueType = Oma.WndwCtrl.Abstractions.Model.ValueType;

namespace Oma.WndwCtrl.Core.Tests.Executors.Transformers;

public sealed class ParserTransformerTests : IDisposable
{
  private readonly CancellationToken _cancelToken;
  private readonly ICliOutputParser _cliOutputParserMock = Substitute.For<ICliOutputParser>();

  private readonly ParserTransformer _instance;

  private Func<Either<Error, ParserResult>> _mockedParseResultFunc;

  private Either<FlowError, TransformationOutcome>? _result;

  private ParserTransformation _transformationInput = new()
  {
    Statements = ["not used - it's mocked",],
    Cardinality = Cardinality.Single,
    ValueType = ValueType.String,
  };

  public ParserTransformerTests()
  {
    _cancelToken = TestContext.Current.CancellationToken;

    _mockedParseResultFunc = () => Right<ParserResult>(["Default Text Value",]);

    _cliOutputParserMock.Parse(Arg.Any<string>(), Arg.Any<string>())
      .Returns(_ => _mockedParseResultFunc());

    _instance = new ParserTransformer(_cliOutputParserMock);
  }

  public void Dispose()
  {
    _result?.Dispose();
  }

  [Fact]
  public async Task SanityCheck()
  {
    await ExecuteTransformerAsync();
    SatisfiesRight<string>(to => to.Outcome.Should().Be("Default Text Value"));
  }

  [Fact]
  public async Task ShouldSuccessfullyParseSingleBool()
  {
    _transformationInput = _transformationInput with
    {
      ValueType = ValueType.Boolean,
    };

    _mockedParseResultFunc = () => Right<ParserResult>([true,]);

    await ExecuteTransformerAsync();

    SatisfiesRight<bool>(to => to.Outcome.Should().BeTrue());
  }

  [Fact]
  public async Task ShouldSuccessfullyParseSingleString()
  {
    const string expected = "this is a string override";

    _transformationInput = _transformationInput with
    {
      ValueType = ValueType.String,
    };

    _mockedParseResultFunc = () => Right<ParserResult>([expected,]);

    await ExecuteTransformerAsync();

    SatisfiesRight<string>(to => to.Outcome.Should().Be(expected));
  }

  [Fact]
  public async Task ShouldSuccessfullyParseSingleLong()
  {
    const long expected = (long)int.MaxValue + 1;

    _transformationInput = _transformationInput with
    {
      ValueType = ValueType.Long,
    };

    _mockedParseResultFunc = () => Right<ParserResult>([expected,]);

    await ExecuteTransformerAsync();

    SatisfiesRight<long>(to => to.Outcome.Should().Be(expected));
  }

  [Fact]
  public async Task ShouldSuccessfullyParseSingleDouble()
  {
    const double expected = 12345.6789d;

    _transformationInput = _transformationInput with
    {
      ValueType = ValueType.Decimal,
    };

    _mockedParseResultFunc = () => Right<ParserResult>([expected,]);

    await ExecuteTransformerAsync();

    SatisfiesRight<decimal>(to => to.Outcome.Should().Be((decimal)expected));
  }

  private async Task ExecuteTransformerAsync()
  {
    using TransformationOutcome emptyOutcome = new();

    _result = await _instance.TransformCommandOutcomeAsync(
      _transformationInput,
      Right(emptyOutcome),
      _cancelToken
    );
  }

  private void SatisfiesRight<T>(params Action<TransformationOutcome<T>>[] assertions)
  {
    using AssertionScope scope = new();

    _result.Should().NotBeNull();

    _result?.Match(
      Right: val =>
      {
        if (val is not TransformationOutcome<T> casted)
        {
          throw new InvalidOperationException(
            $"Expected outcome to be of type {typeof(T).Name}, but it was {val.GetType()}."
          );
        }

        foreach (Action<TransformationOutcome<T>> assertion in assertions) assertion(casted);
      },
      Left: val => val.Should().BeNull()
    );
  }
}