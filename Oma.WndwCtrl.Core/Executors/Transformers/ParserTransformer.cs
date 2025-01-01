using LanguageExt;
using LanguageExt.Common;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.Core.Model.Transformations;

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
          IEnumerable<object> values = parsedOutcome.Outcome!;

          parseResult = cliOutputParser.Parse(commandText, values);
        }
        else
        {
          parseResult = cliOutputParser.Parse(commandText, outcome.OutcomeRaw);
        }

        return parseResult.MapLeft<FlowError>(err => new TransformationError(err));
      },
      Left: err => err
    ).BiBind<FlowError, TransformationOutcome>(
      err => err,
      parseResult => new TransformationOutcome<ParserResult>(parseResult, success: true)
    );

    return Task.FromResult(result);
  }
}