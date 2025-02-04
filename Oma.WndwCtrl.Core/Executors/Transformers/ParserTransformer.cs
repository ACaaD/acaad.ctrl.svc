using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LanguageExt;
using LanguageExt.Common;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Errors.Transformations;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.Core.Errors.Transformations;
using Oma.WndwCtrl.Core.Model.Transformations;
using static LanguageExt.Prelude;
using ValueType = Oma.WndwCtrl.Abstractions.Model.ValueType;

namespace Oma.WndwCtrl.Core.Executors.Transformers;

public class ParserTransformer(ICliOutputParser cliOutputParser) : IOutcomeTransformer<ParserTransformation>
{
  public Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
    ParserTransformation transformation,
    Either<FlowError, TransformationOutcome> transformationOutcome,
    CancellationToken cancelToken = default
  )
  {
    Either<FlowError, TransformationOutcome> result = transformationOutcome.BiBind(
      Right: outcome =>
      {
        // Cannot use empty string for concatenation, otherwise comments will break a snippet,
        // Since all commands after the comment are treated as a comment as well.
        string commandText = string.Join(Environment.NewLine, transformation.Statements);

        Either<Error, ParserResult> parseResult;

        if (outcome is TransformationOutcome<ParserResult> parsedOutcome)
        {
          ParserResult values = parsedOutcome.Outcome!;

          parseResult = cliOutputParser.Parse(commandText, values.ToList());
        }
        else
        {
          parseResult = cliOutputParser.Parse(commandText, outcome.OutcomeRaw);
        }

        return parseResult.MapLeft<FlowError>(err => new ParserTransformationError(err));
      },
      Left: err => err
    ).BiBind(
      err => err,
      outcome => MapOutcome(transformation, outcome)
    );

    return Task.FromResult(result);
  }

  [MustDisposeResource]
  [SuppressMessage(
    "ReSharper",
    "NotDisposedResource",
    Justification = "Disposable is returned to the caller."
  )]
  private static Either<FlowError, TransformationOutcome> MapOutcome(
    ParserTransformation transformation,
    ParserResult parserResult
  )
  {
    if (transformation.Cardinality == Cardinality.Multiple)
    {
      // Don't verify value types. parsing the values is a nightmare because of the multiple levels possible
      // It _probably_ confuses the user anyway.
      return new TransformationOutcome<ParserResult>(parserResult, success: true);
    }

    if (parserResult.Count != 1)
    {
      return Left<FlowError>(new MismatchedCardinalityTransformationError(transformation.Cardinality));
    }

    object single = parserResult.Single();

    Either<FlowError, TransformationOutcome> tmp = ParseByValueType(transformation, single);
    return tmp.Map<TransformationOutcome>(oc => oc);
  }

  [MustDisposeResource]
  private static Either<FlowError, TransformationOutcome> ParseByValueType(
    ParserTransformation transformation,
    object single
  ) => transformation.ValueType switch
  {
    ValueType.Boolean => ParseBool(single),
    ValueType.String => ParseString(single),
    ValueType.Long => ParseLong(single),
    ValueType.Decimal => ParseDecimal(single),
    var _ => throw new ArgumentOutOfRangeException(
      $"ValueType {transformation.ValueType} is not supported. This is a programming error."
    ),
  };

  [MustDisposeResource]
  private static Either<FlowError, TransformationOutcome> ParseBool(object singleOutcome)
    => Parse<bool>(
      ValueType.Boolean,
      singleOutcome,
      asString => bool.TryParse(asString, out bool result)
        ? result
        : null
    );

  [MustDisposeResource]
  private static Either<FlowError, TransformationOutcome> ParseString(object singleOutcome)
  {
    if (singleOutcome is string matching)
    {
      return new TransformationOutcome<string>(matching);
    }

    string? asString = singleOutcome.ToString();

    if (string.IsNullOrWhiteSpace(asString))
    {
      return Left<FlowError>(new ValueEmptyTransformationError(ValueType.String));
    }

    return new TransformationOutcome<string>(asString);
  }

  [MustDisposeResource]
  private static Either<FlowError, TransformationOutcome> ParseLong(object singleOutcome)
    => Parse<long>(
      ValueType.Long,
      singleOutcome,
      asString => long.TryParse(asString, out long result)
        ? result
        : null
    );

  [MustDisposeResource]
  private static Either<FlowError, TransformationOutcome> ParseDecimal(object singleOutcome)
    => Parse<decimal>(
      ValueType.Decimal,
      singleOutcome,
      asString => decimal.TryParse(asString, out decimal result)
        ? result
        : null
    );

  [MustDisposeResource]
  private static Either<FlowError, TransformationOutcome> Parse<T>(
    ValueType valueType,
    object singleOutcome,
    Func<string, T?> parse
  ) where T : struct
  {
    if (singleOutcome is T matching)
    {
      return new TransformationOutcome<T>(matching);
    }

    string? asString = singleOutcome.ToString();

    if (string.IsNullOrWhiteSpace(asString))
    {
      return Left<FlowError>(new ValueEmptyTransformationError(valueType));
    }

    T? parsed = parse(asString);

    if (parsed.HasValue)
    {
      return new TransformationOutcome<T>(parsed.Value);
    }

    return Left<FlowError>(
      new ValueTypeMismatchTransformationError(valueType, singleOutcome.GetType())
    );
  }
}