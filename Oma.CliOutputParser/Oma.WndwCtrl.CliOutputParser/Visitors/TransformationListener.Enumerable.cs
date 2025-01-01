using Oma.WndwCtrl.CliOutputParser.Model;

namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public partial class TransformationListener
{
  private static NestedEnumerable MapItemsRecursive(NestedEnumerable nestedList, Func<object, object> map)
  {
    if (nestedList.IsNested)
    {
      return new NestedEnumerable(
        nestedList.Children.Select(l => MapItemsRecursive(l, map)),
        isNested: true
      );
    }

    return NestedEnumerable.FromEnumerableInternal(nestedList.Select(map), nestedList.IsNested);
  }

  private static NestedEnumerable UnfoldItemsRecursive(
    NestedEnumerable nestedList,
    Func<IEnumerable<object>, IEnumerable<IEnumerable<object>>> unfold
  )
  {
    if (nestedList.IsNested)
    {
      IEnumerable<NestedEnumerable> children = nestedList.Children
        .Select(l => new NestedEnumerable(UnfoldItemsRecursive(l, unfold), isNested: true));

      return new NestedEnumerable(children, isNested: true);
    }

    IEnumerable<IEnumerable<object>> unfoldInter = unfold(nestedList);
    IEnumerable<NestedEnumerable> select = unfoldInter.Select(NestedEnumerable.ForChild);

    NestedEnumerable unfoldResult = new(select, isNested: true);

    return unfoldResult;
  }

  private static object? FoldItemsRecursive(
    NestedEnumerable nestedList,
    Func<IEnumerable<object>, object?> fold
  )
  {
    if (!nestedList.IsNested)
    {
      return fold(nestedList);
    }

    List<object> result = nestedList.Children.Select(l => FoldItemsRecursive(l, fold))
      .Where(obj => obj is not null)
      .Select(obj => obj!)
      .ToList();

    // TODO: Check this function.
    // If only the first item is used, then it could lead to problems in multi-nesting
    // if there is no value for the first item, but for the others (IsNested determined incorrectly)

    bool isNested = result.FirstOrDefault() is NestedEnumerable { IsNested: true, };
    return NestedEnumerable.FromEnumerableInternal(result, isNested);
  }

  private void StoreFoldResult(object? result)
  {
    if (result is NestedEnumerable results)
    {
      CurrentValues = results;
    }
    else
    {
      List<object> newList = [];

      if (result is not null)
      {
        newList.Add(result);
      }

      CurrentValues = NestedEnumerable.FromEnumerableInternal(newList.AsEnumerable(), isNested: false);
    }
  }
}