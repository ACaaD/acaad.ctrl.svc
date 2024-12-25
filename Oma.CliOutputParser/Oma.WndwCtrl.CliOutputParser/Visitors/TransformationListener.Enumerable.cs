namespace Oma.WndwCtrl.CliOutputParser.Visitors;

public partial class TransformationListener
{
    private static IEnumerable<object> MapItemsRecursive(IEnumerable<object> nestedList, Func<object, object> map)
    {
        if (nestedList is IEnumerable<IEnumerable<object>> list)
        {
            return list.Select(l => MapItemsRecursive(l, map));
        }

        return nestedList.Select(map);
    }

    private static IEnumerable<IEnumerable<object>> UnfoldItemsRecursive(IEnumerable<object> nestedList, Func<IEnumerable<object>, IEnumerable<IEnumerable<object>>> unfold)
    {
        if (nestedList is IEnumerable<IEnumerable<object>> tst)
        {
            return tst.Select(l => UnfoldItemsRecursive(l, unfold));
        }

        var unfoldResult = unfold(nestedList); 
        return unfoldResult;   
    }

    private static object? FoldItemsRecursive(IEnumerable<object> nestedList, Func<IEnumerable<object>, object?> fold)
    {
        if (nestedList is IEnumerable<IEnumerable<object>> tst)
        {
            return tst
                .Select(l => FoldItemsRecursive(l, fold))
                .Where(v => v is not null);
        }

        return fold(nestedList);
    }
    
    private void StoreFoldResult(object? result)
    {
        if (result is IEnumerable<object> results)
        {
            CurrentValues = results;
        }
        else
        {
            var newList = new List<object>();

            if (result is not null)
            {
                newList.Add(result);
            }
            
            CurrentValues = newList.AsEnumerable();
        }
    }
}