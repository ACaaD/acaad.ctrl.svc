using Antlr4.Runtime.Tree;
using LanguageExt;
using LanguageExt.Common;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.CliOutputParser.Visitors;
using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.CliOutputParser;

public class CliOutputParserImpl(
  IParserLogger parserLogger,
  TransformationTreeCache treeCache
) : ICliOutputParser
{
  public Either<Error, ParserResult> Parse(string transformation, string text)
  {
    return Parse(transformation, Build);

    TransformationListener Build()
    {
      return new TransformationListener(parserLogger, text);
    }
  }

  public Either<Error, ParserResult> Parse(string transformation, IList<object> values)
  {
    return Parse(transformation, Build);

    TransformationListener Build()
    {
      return new TransformationListener(parserLogger, values);
    }
  }

  private Either<Error, ParserResult> Parse(
    string transformation,
    Func<TransformationListener> transformationListenerFactory
  )
  {
    Either<Error, Grammar.CliOutputParser.TransformationContext>
      treeOrError = treeCache.GetOrCreateTree(transformation);

    return treeOrError.Bind(
      tree => ExecuteParser(transformationListenerFactory, tree)
    );
  }

  private static Either<Error, ParserResult> ExecuteParser(
    Func<TransformationListener> transformationListenerFactory,
    Grammar.CliOutputParser.TransformationContext tree
  )
  {
    TransformationListener listener = transformationListenerFactory();

    ParseTreeWalker walker = new();
    Exception? thrownException;

    try
    {
      walker.Walk(listener, tree);
    }
    catch (Exception ex)
    {
      thrownException = ex;
    }

    if (listener.Error is not null)
    {
      return listener.Error;
    }

    List<object> enumeratedList = listener.CurrentValues.ToList();

    if (enumeratedList.Count == 1)
    {
      return Right<ParserResult>([enumeratedList.Single(),]);
    }

    ParserResult result = [];
    result.AddRange(enumeratedList);

    return result;
  }
}