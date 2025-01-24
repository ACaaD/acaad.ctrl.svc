using Antlr4.Runtime.Tree;
using LanguageExt;
using LanguageExt.Common;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.CliOutputParser.Visitors;

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

    return treeOrError.Map(
      tree => ExecuteParser(transformationListenerFactory, tree)
    );
  }

  private static ParserResult ExecuteParser(
    Func<TransformationListener> transformationListenerFactory,
    Grammar.CliOutputParser.TransformationContext tree
  )
  {
    TransformationListener listener = transformationListenerFactory();

    ParseTreeWalker walker = new();
    walker.Walk(listener, tree);

    List<object> enumeratedList = listener.CurrentValues.ToList();

    if (enumeratedList.Count == 1)
    {
      return [enumeratedList.Single(),];
    }

    ParserResult result = [];
    result.AddRange(enumeratedList);

    return result;
  }
}