using System.Collections;
using LanguageExt;

namespace Oma.WndwCtrl.CliOutputParser.Model;

public class NestedEnumerable : IEnumerable<object>
{
    private List<object> _enumerable { get; init; }
    
    public bool IsNested { get; init; }

    public NestedEnumerable()
    {
        _enumerable = new();
    }
    
    public NestedEnumerable(NestedEnumerable enumerable, bool isNested) : this()
    {
        _enumerable.AddRange(enumerable);
        IsNested = isNested;
    }
    
    public NestedEnumerable(IEnumerable<NestedEnumerable> children, bool isNested) : this()
    {
        _enumerable.AddRange(children);
        IsNested = isNested;
    }

    public static NestedEnumerable FromString(string input)
        => new()
        {
            _enumerable = [input],
            IsNested = false,
        };

    public static NestedEnumerable ForChild(IEnumerable<object> child)
        => new()
        {
            _enumerable = child.ToList(),
            IsNested = false,
        };
    
    internal static NestedEnumerable FromEnumerableInternal(IEnumerable<object> enumerable, bool isNested)
        => new()
        {
            _enumerable = enumerable.ToList(),
            IsNested = isNested,
        };
    
    public static NestedEnumerable FromEnumerable(IEnumerable<object> enumerable)
    {
        var enumerated = enumerable.ToList();
        bool isNestedCalc = enumerated.Any(item => item is IEnumerable and not IEnumerable<char>);
        
        var res = new NestedEnumerable()
        {
            _enumerable = enumerated.ToList(),
            IsNested = isNestedCalc,
        };

        return res;
    }

    public IEnumerator<object> GetEnumerator() => _enumerable.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public IEnumerable<NestedEnumerable> Children => IsNested 
        ? _enumerable.OfType<NestedEnumerable>()
        : throw new InvalidOperationException("Children was accessed but enum is not nested.");
}