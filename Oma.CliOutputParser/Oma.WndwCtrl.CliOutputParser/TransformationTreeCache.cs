using System.Collections.Concurrent;
using System.Diagnostics;
using Antlr4.Runtime;
using LanguageExt;
using LanguageExt.Common;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.CliOutputParser.Grammar;
using Oma.WndwCtrl.CliOutputParser.Metrics;

namespace Oma.WndwCtrl.CliOutputParser;

public class TransformationTreeCache(ParserMetrics metrics)
{
  private readonly ConcurrentDictionary<string, Either<Error, Grammar.CliOutputParser.TransformationContext>>
    _treeCache = new();

  internal Either<Error, Grammar.CliOutputParser.TransformationContext> GetOrCreateTree(string transformation)
  {
    Stopwatch swTreeGen = Stopwatch.StartNew();

    string transformationHint = transformation.Split(Environment.NewLine).First();
    string transformationId = GetMd5(transformation);

    try
    {
      return _treeCache.GetOrAdd(
        transformationId,
        _ => GenerateParseTree(transformation, transformationId, transformationHint)
      );
    }
    finally
    {
      metrics.RecordParseTreeGenerationDuration(transformationId, transformationHint, swTreeGen.Measure());
    }
  }

  private Either<Error, Grammar.CliOutputParser.TransformationContext> GenerateParseTree(
    string transformation,
    string transformationId,
    string transformationHint
  )
  {
    CollectingErrorListener errorListener = new();
    AntlrInputStream charStream = new(transformation);
    CliOutputLexer lexer = new(charStream);

    lexer.AddErrorListener(errorListener);

    CommonTokenStream tokenStream = new(lexer);

    CliOutputParser.Grammar.CliOutputParser parser = new(tokenStream);
    parser.AddErrorListener(errorListener);

    CliOutputParser.Grammar.CliOutputParser.TransformationContext tree = parser.transformation();

    if (errorListener.Errors.Count > 0)
    {
      metrics.RecordInvalidTransformation(transformationId, transformationHint);
      return Error.Many(errorListener.Errors.Cast<Error>().ToArray());
    }

    metrics.RecordTransformationCreation(transformationId, transformationHint);

    return tree;
  }

  private static string GetMd5(string input)
  {
    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
    byte[] hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);

    return Convert.ToHexString(hashBytes);
  }
}