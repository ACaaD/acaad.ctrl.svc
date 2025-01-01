using System.Collections;

namespace Oma.WndwCtrl.CliOutputParser.Model;

public class NestedEnumerable : IEnumerable<object>
{
  public NestedEnumerable()
  {
    _enumerable = new List<object>();
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

  private List<object> _enumerable { get; init; }

  public bool IsNested { get; init; }

  public IEnumerable<NestedEnumerable> Children => IsNested
    ? _enumerable.OfType<NestedEnumerable>()
    : throw new InvalidOperationException("Children was accessed but enum is not nested.");

  public IEnumerator<object> GetEnumerator()
  {
    return _enumerable.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  public static NestedEnumerable FromString(string input)
  {
    return new NestedEnumerable
    {
      _enumerable = [input,],
      IsNested = false,
    };
  }

  public static NestedEnumerable ForChild(IEnumerable<object> child)
  {
    return new NestedEnumerable
    {
      _enumerable = child.ToList(),
      IsNested = false,
    };
  }

  internal static NestedEnumerable FromEnumerableInternal(IEnumerable<object> enumerable, bool isNested)
  {
    return new NestedEnumerable
    {
      _enumerable = enumerable.ToList(),
      IsNested = isNested,
    };
  }

  public static NestedEnumerable FromEnumerable(IEnumerable<object> enumerable)
  {
    List<object>? enumerated = enumerable.ToList();
    bool isNestedCalc = enumerated.Any(item => item is IEnumerable and not IEnumerable<char>);

    NestedEnumerable? res = new()
    {
      _enumerable = enumerated.ToList(),
      IsNested = isNestedCalc,
    };

    return res;
  }
}